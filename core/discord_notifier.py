"""Send notifications to a Discord webhook. Kept separate from the Drive logic so it can be
used both by track.py (change report) and retrieve.py (download result report).

Long lists never get silently cut off: they're split across multiple fields, then multiple
embeds, then multiple messages as needed, following Discord's own limits.
"""
import requests
from core import gemini_flavor

# Discord's hard limits (webhook messages).
FIELD_VALUE_LIMIT = 1024
MAX_FIELDS_PER_EMBED = 25
MAX_EMBED_CHARS = 6000
MAX_EMBEDS_PER_MESSAGE = 10
DESCRIPTION_LIMIT = 4096

# Small safety buffers so we never brush right up against a hard limit.
_FIELD_BUFFER = 40
_EMBED_BUFFER = 200


def _lines_files(files):
    return [f"- [{f['relative_path']}]({f.get('webViewLink', '')})" for f in files]


def _lines_names(names):
    return [f"- {n}" for n in names]


# How many filenames get surfaced to Gemini per category, so it can react to *something*
# specific without dumping potentially huge lists into the prompt. "Secukupnya" — just enough
# for flavor, not a full report (that's what the Discord fields above are for).
_SAMPLE_NAMES_LIMIT = 3
_SAMPLE_NAME_MAXLEN = 60


def _sample_names(names, limit=_SAMPLE_NAMES_LIMIT):
    """Turn a list of names/paths into a short 'a, b, c, +N more' string for the Gemini
    prompt. Each name is truncated if it's absurdly long. Returns None for an empty list."""
    names = list(names)
    if not names:
        return None
    shown = [n if len(n) <= _SAMPLE_NAME_MAXLEN else n[:_SAMPLE_NAME_MAXLEN - 1] + "…" for n in names[:limit]]
    text = ", ".join(shown)
    remaining = len(names) - len(shown)
    if remaining > 0:
        text += f", +{remaining} more"
    return text


def _summary_part(label, items, names_fn):
    """Build one '<label>: N (a, b, +N more)' chunk for the Gemini summary, or '<label>: 0'
    if empty."""
    if not items:
        return f"{label}: 0"
    sample = _sample_names(names_fn(items))
    return f"{label}: {len(items)} ({sample})"


def _chunk_by_length(lines, limit=FIELD_VALUE_LIMIT - _FIELD_BUFFER):
    """Group lines into chunks that each join to under `limit` chars."""
    chunks, current, current_len = [], [], 0
    for line in lines:
        cost = len(line) + 1  # +1 for the newline joining it to the next line
        if current and current_len + cost > limit:
            chunks.append(current)
            current, current_len = [], 0
        current.append(line)
        current_len += cost
    if current:
        chunks.append(current)
    return chunks


def _fields_for(label, lines):
    """Turn a list of markdown lines into one or more Discord fields, each safely under the
    1024-char value limit. If it takes more than one field, each is numbered (i/N)."""
    if not lines:
        return [{"name": label, "value": "_none_", "inline": False}]
    chunks = _chunk_by_length(lines)
    total = len(chunks)
    fields = []
    for i, chunk in enumerate(chunks, start=1):
        name = label if total == 1 else f"{label} ({i}/{total})"
        fields.append({"name": name, "value": "\n".join(chunk), "inline": False})
    return fields


def _split_into_embeds(title, color, fields, description=None):
    """Group fields into one or more embeds, respecting the 25-fields and 6000-char limits.
    Only the first embed gets the description; later parts are labeled (part i/N)."""
    groups, current, current_chars = [], [], len(title) + len(description or "")
    for f in fields:
        f_chars = len(f["name"]) + len(f["value"])
        if current and (
            len(current) >= MAX_FIELDS_PER_EMBED
            or current_chars + f_chars > MAX_EMBED_CHARS - _EMBED_BUFFER
        ):
            groups.append(current)
            current, current_chars = [], len(title) + len(description or "")
        current.append(f)
        current_chars += f_chars
    if current:
        groups.append(current)

    total = len(groups)
    embeds = []
    for i, flds in enumerate(groups, start=1):
        embed = {
            "title": title if total == 1 else f"{title} (part {i}/{total})",
            "color": color,
            "fields": flds,
        }
        if description and i == 1:
            embed["description"] = description[:DESCRIPTION_LIMIT]
        embeds.append(embed)
    return embeds


def _send_embeds(webhook_url, embeds):
    """Post embeds to the webhook, batching in groups of 10 (Discord's per-message max)."""
    for i in range(0, len(embeds), MAX_EMBEDS_PER_MESSAGE):
        batch = embeds[i:i + MAX_EMBEDS_PER_MESSAGE]
        r = requests.post(webhook_url, json={"embeds": batch}, timeout=15)
        if not r.ok:
            # Surface Discord's actual error message in the Actions log instead of a bare 400.
            print("Discord response body:", r.text)
        r.raise_for_status()


def send_track_notification(
    webhook_url, pair_name, new_files, changed_files, deleted_files,
    gemini_api_key=None, gemini_model=None, gemini_persona=None,
):
    """Report the result of a change check. Not sent if there are no changes at all."""
    if not (new_files or changed_files or deleted_files):
        return

    fields = []
    if new_files:
        fields += _fields_for(f"🆕 New files ({len(new_files)})", _lines_files(new_files))
    if changed_files:
        fields += _fields_for(f"✏️ Changed files ({len(changed_files)})", _lines_files(changed_files))
    if deleted_files:
        lines = _lines_files(deleted_files) + ["_(not deleted automatically anywhere)_"]
        fields += _fields_for(f"🗑️ Removed from Drive ({len(deleted_files)})", lines)

    summary = (
        f"Project: {pair_name}. "
        + _summary_part("New", new_files, lambda fs: [f["relative_path"] for f in fs]) + "; "
        + _summary_part("Changed", changed_files, lambda fs: [f["relative_path"] for f in fs]) + "; "
        + _summary_part("Removed", deleted_files, lambda fs: [f["relative_path"] for f in fs])
    )
    flavor = gemini_flavor.generate_flavor_text(gemini_api_key, summary, gemini_model, gemini_persona)

    embeds = _split_into_embeds(f"📁 Drive update — {pair_name}", 0x4285F4, fields, flavor)
    _send_embeds(webhook_url, embeds)


def send_retrieve_notification(
    webhook_url, pair_name, retrieved_names, updated_names, skipped_names,
    gemini_api_key=None, gemini_model=None, gemini_persona=None,
):
    """Report the result of a retrieve/download run. Always fires when there's something to
    report (something downloaded, updated, and/or skipped). webhook_url is required by
    retrieve.py.
    - retrieved_names: brand-new files downloaded for the first time.
    - updated_names: files that existed before and got re-downloaded because the SAME Drive
      file changed (overwrote the local copy).
    - skipped_names: brand-new Drive files whose name collided with some OTHER local file.
    """
    if not (retrieved_names or updated_names or skipped_names):
        return

    fields = []
    if retrieved_names:
        fields += _fields_for(f"⬇️ Downloaded ({len(retrieved_names)})", _lines_names(retrieved_names))
    if updated_names:
        fields += _fields_for(
            f"🔄 Updated, file changed in Drive ({len(updated_names)})", _lines_names(updated_names)
        )
    if skipped_names:
        fields += _fields_for(
            f"⏭️ Skipped, name already exists ({len(skipped_names)})", _lines_names(skipped_names)
        )

    summary = (
        f"Project: {pair_name}. "
        + _summary_part("Downloaded", retrieved_names, lambda ns: ns) + "; "
        + _summary_part("Updated", updated_names, lambda ns: ns) + "; "
        + _summary_part("Skipped", skipped_names, lambda ns: ns)
    )
    flavor = gemini_flavor.generate_flavor_text(gemini_api_key, summary, gemini_model, gemini_persona)

    embeds = _split_into_embeds(f"⬇️ Retrieve result — {pair_name}", 0x57F287, fields, flavor)
    _send_embeds(webhook_url, embeds)
