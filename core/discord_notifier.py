"""Send notifications to a Discord webhook.

Used by track.py (Drive change reports) and retrieve.py (download result reports).

Long lists are never silently truncated:
    fields -> embeds -> webhook messages

The implementation deliberately stays below Discord's documented limits so that
small additions such as part numbers or descriptions cannot push an embed over
the limit.
"""

import requests

from core import gemini_flavor


# ---------------------------------------------------------------------------
# Discord limits
# ---------------------------------------------------------------------------

FIELD_VALUE_LIMIT = 1024
FIELD_NAME_LIMIT = 256
MAX_FIELDS_PER_EMBED = 25
MAX_EMBED_CHARS = 6000
MAX_EMBEDS_PER_MESSAGE = 10
DESCRIPTION_LIMIT = 4096
TITLE_LIMIT = 256

# Safety margins.
_FIELD_BUFFER = 40
_EMBED_BUFFER = 200

# Keep individual values comfortably below Discord's limit.
SAFE_FIELD_VALUE_LIMIT = FIELD_VALUE_LIMIT - _FIELD_BUFFER
SAFE_EMBED_LIMIT = MAX_EMBED_CHARS - _EMBED_BUFFER


# ---------------------------------------------------------------------------
# Formatting helpers
# ---------------------------------------------------------------------------

def _safe_title(title):
    """Keep an embed title within Discord's title limit."""
    if len(title) <= TITLE_LIMIT:
        return title
    return title[:TITLE_LIMIT - 1] + "…"


def _lines_files(files):
    """Convert Drive file dictionaries into Discord markdown lines."""
    lines = []

    for f in files:
        path = str(f.get("relative_path", ""))
        link = str(f.get("webViewLink", ""))

        if link:
            lines.append(f"- [{path}]({link})")
        else:
            lines.append(f"- {path}")

    return lines


def _lines_names(names):
    """Convert plain names into Discord markdown lines."""
    return [f"- {name}" for name in names]


# ---------------------------------------------------------------------------
# Gemini sampling
# ---------------------------------------------------------------------------

_SAMPLE_NAMES_LIMIT = 3
_SAMPLE_NAME_MAXLEN = 60


def _sample_names(names, limit=_SAMPLE_NAMES_LIMIT):
    """Create a short representative list for the Gemini prompt."""
    names = list(names)

    if not names:
        return None

    shown = []

    for name in names[:limit]:
        name = str(name)

        if len(name) > _SAMPLE_NAME_MAXLEN:
            name = name[:_SAMPLE_NAME_MAXLEN - 1] + "…"

        shown.append(name)

    text = ", ".join(shown)

    remaining = len(names) - len(shown)

    if remaining > 0:
        text += f", +{remaining} more"

    return text


def _summary_part(label, items, names_fn):
    """Build a compact summary for Gemini."""
    if not items:
        return f"{label}: 0"

    sample = _sample_names(names_fn(items))

    return f"{label}: {len(items)} ({sample})"


# ---------------------------------------------------------------------------
# Discord field splitting
# ---------------------------------------------------------------------------

def _split_long_line(line, limit=SAFE_FIELD_VALUE_LIMIT):
    """Split a single pathological line that exceeds the field value limit."""
    line = str(line)

    if len(line) <= limit:
        return [line]

    chunks = []

    while len(line) > limit:
        chunks.append(line[:limit])
        line = line[limit:]

    if line:
        chunks.append(line)

    return chunks


def _chunk_by_length(lines, limit=SAFE_FIELD_VALUE_LIMIT):
    """Group lines into chunks that fit inside a Discord field value."""
    chunks = []
    current = []
    current_len = 0

    for original_line in lines:
        # A single Drive path/URL can theoretically be enormous.
        split_lines = _split_long_line(original_line, limit)

        for line in split_lines:
            cost = len(line)

            # Account for the newline between entries.
            if current:
                cost += 1

            if current and current_len + cost > limit:
                chunks.append(current)
                current = []
                current_len = 0
                cost = len(line)

            current.append(line)
            current_len += cost

    if current:
        chunks.append(current)

    return chunks


def _fields_for(label, lines):
    """Turn lines into Discord fields, respecting the 1024-char value limit."""
    label = _safe_title(label)

    if not lines:
        return [
            {
                "name": label[:FIELD_NAME_LIMIT],
                "value": "_none_",
                "inline": False,
            }
        ]

    chunks = _chunk_by_length(lines)

    total = len(chunks)
    fields = []

    for i, chunk in enumerate(chunks, start=1):
        if total == 1:
            name = label
        else:
            name = f"{label} ({i}/{total})"

        name = name[:FIELD_NAME_LIMIT]

        value = "\n".join(chunk)

        # Defensive check. This should already be guaranteed by
        # _chunk_by_length(), but never send an invalid Discord field.
        if len(value) > FIELD_VALUE_LIMIT:
            value = value[:FIELD_VALUE_LIMIT - 1] + "…"

        fields.append(
            {
                "name": name,
                "value": value,
                "inline": False,
            }
        )

    return fields


# ---------------------------------------------------------------------------
# Embed splitting
# ---------------------------------------------------------------------------

def _embed_text_size(embed):
    """Calculate the text counted toward Discord's 6000-character embed limit."""
    total = 0

    total += len(str(embed.get("title", "")))
    total += len(str(embed.get("description", "")))

    for field in embed.get("fields", []):
        total += len(str(field.get("name", "")))
        total += len(str(field.get("value", "")))

    total += len(str(embed.get("footer", {}).get("text", "")))

    total += len(str(embed.get("author", {}).get("name", "")))

    return total


def _make_embed(title, color, fields=None, description=None):
    """Construct an embed."""
    embed = {
        "title": _safe_title(title),
        "color": color,
        "fields": fields or [],
    }

    if description:
        # Discord's description has a hard 4096-character limit.
        embed["description"] = description[:DESCRIPTION_LIMIT]

    return embed


def _split_into_embeds(title, color, fields, description=None):
    """Split fields into embeds while respecting Discord's hard limits.

    Every resulting embed is checked against the 6000-character limit before
    being returned.
    """

    title = _safe_title(title)

    # The description belongs only on the first embed.
    description = (description or "")[:DESCRIPTION_LIMIT]

    embeds = []

    current_fields = []
    first_embed = True

    def build_current():
        desc = description if first_embed else None

        return _make_embed(
            title=title,
            color=color,
            fields=current_fields,
            description=desc,
        )

    for field in fields:
        candidate_fields = current_fields + [field]

        candidate = _make_embed(
            title=title,
            color=color,
            fields=candidate_fields,
            description=description if first_embed else None,
        )

        too_many_fields = len(candidate_fields) > MAX_FIELDS_PER_EMBED
        too_many_chars = _embed_text_size(candidate) > SAFE_EMBED_LIMIT

        if current_fields and (too_many_fields or too_many_chars):
            embeds.append(build_current())

            current_fields = [field]
            first_embed = False

        else:
            current_fields = candidate_fields

    if current_fields or not embeds:
        embeds.append(build_current())

    # Add part numbering if there is more than one embed.
    total = len(embeds)

    if total > 1:
        numbered = []

        for i, embed in enumerate(embeds, start=1):
            numbered_title = _safe_title(f"{title} (part {i}/{total})")

            numbered_embed = dict(embed)
            numbered_embed["title"] = numbered_title

            # Re-check the final title after adding "(part i/N)".
            # If somehow that pushes the embed over our safety limit,
            # rebuild it without the description where necessary.
            if _embed_text_size(numbered_embed) > SAFE_EMBED_LIMIT:
                if "description" in numbered_embed:
                    numbered_embed.pop("description")

            numbered.append(numbered_embed)

        embeds = numbered

    # Final defensive validation.
    #
    # This should never fire because all splitting above is conservative.
    # If it somehow does, split fields one-by-one rather than sending an
    # invalid webhook request.
    final_embeds = []

    for embed in embeds:
        if (
            len(embed.get("fields", [])) <= MAX_FIELDS_PER_EMBED
            and _embed_text_size(embed) <= SAFE_EMBED_LIMIT
        ):
            final_embeds.append(embed)
            continue

        # Extremely defensive fallback.
        fields = embed.get("fields", [])

        for field in fields:
            fallback = _make_embed(
                title=embed.get("title", title),
                color=color,
                fields=[field],
            )

            final_embeds.append(fallback)

    return final_embeds


# ---------------------------------------------------------------------------
# Webhook sending
# ---------------------------------------------------------------------------

def _batch_embeds(embeds):
    """Group embeds into webhook-message batches.

    Discord enforces two independent limits per message:
      - at most 10 embeds
      - the COMBINED character count across all embeds in the message
        (title + description + all field names/values + footer + author)
        must not exceed 6000

    Each individual embed produced by _split_into_embeds already respects
    SAFE_EMBED_LIMIT on its own, but that says nothing about how many of
    those embeds get grouped into a single webhook request. Several
    individually-safe embeds sent together can still exceed the message-level
    6000-character budget, so batching has to track a running total, not just
    a count of 10.
    """
    batches = []
    current = []
    current_size = 0

    for embed in embeds:
        size = _embed_text_size(embed)

        too_many = len(current) + 1 > MAX_EMBEDS_PER_MESSAGE
        too_big = current and (current_size + size > SAFE_EMBED_LIMIT)

        if current and (too_many or too_big):
            batches.append(current)
            current = []
            current_size = 0

        current.append(embed)
        current_size += size

    if current:
        batches.append(current)

    return batches


def _send_embeds(webhook_url, embeds):
    """Send embeds in batches that respect Discord's per-message limits.

    A "batch" here is capped both by count (max 10 embeds per message) and
    by combined character size (max ~6000 chars across every embed in the
    message), matching how Discord actually validates incoming webhook
    payloads.
    """

    if not embeds:
        return

    for batch in _batch_embeds(embeds):
        payload = {
            "embeds": batch,
        }

        response = requests.post(
            webhook_url,
            json=payload,
            timeout=15,
        )

        if not response.ok:
            print("Discord response body:", response.text)

        response.raise_for_status()


# ---------------------------------------------------------------------------
# Track notification
# ---------------------------------------------------------------------------

def send_track_notification(
    webhook_url,
    pair_name,
    new_files,
    changed_files,
    deleted_files,
    gemini_api_key=None,
    gemini_model=None,
    gemini_persona=None,
):
    """Report a Google Drive change check.

    Does nothing when there are no changes.
    """

    if not (new_files or changed_files or deleted_files):
        return

    fields = []

    if new_files:
        fields += _fields_for(
            f"🆕 New files ({len(new_files)})",
            _lines_files(new_files),
        )

    if changed_files:
        fields += _fields_for(
            f"✏️ Changed files ({len(changed_files)})",
            _lines_files(changed_files),
        )

    if deleted_files:
        deleted_lines = _lines_files(deleted_files)
        deleted_lines.append("_(not deleted automatically anywhere)_")

        fields += _fields_for(
            f"🗑️ Removed from Drive ({len(deleted_files)})",
            deleted_lines,
        )

    summary = (
        f"Project: {pair_name}. "
        + _summary_part(
            "New",
            new_files,
            lambda fs: [f.get("relative_path", "") for f in fs],
        )
        + "; "
        + _summary_part(
            "Changed",
            changed_files,
            lambda fs: [f.get("relative_path", "") for f in fs],
        )
        + "; "
        + _summary_part(
            "Removed",
            deleted_files,
            lambda fs: [f.get("relative_path", "") for f in fs],
        )
    )

    flavor = gemini_flavor.generate_flavor_text(
        gemini_api_key,
        summary,
        gemini_model,
        gemini_persona,
    )

    embeds = _split_into_embeds(
        f"📁 Drive update — {pair_name}",
        0x4285F4,
        fields,
        flavor,
    )

    _send_embeds(webhook_url, embeds)


# ---------------------------------------------------------------------------
# Retrieve notification
# ---------------------------------------------------------------------------

def send_retrieve_notification(
    webhook_url,
    pair_name,
    retrieved_names,
    updated_names,
    skipped_names,
    gemini_api_key=None,
    gemini_model=None,
    gemini_persona=None,
):
    """Report a retrieve/download run.

    retrieved_names:
        Brand-new files downloaded for the first time.

    updated_names:
        Existing files re-downloaded because the same Drive file changed.

    skipped_names:
        Brand-new Drive files whose names collided with another local file.
    """

    if not (retrieved_names or updated_names or skipped_names):
        return

    fields = []

    if retrieved_names:
        fields += _fields_for(
            f"⬇️ Downloaded ({len(retrieved_names)})",
            _lines_names(retrieved_names),
        )

    if updated_names:
        fields += _fields_for(
            f"🔄 Updated, file changed in Drive ({len(updated_names)})",
            _lines_names(updated_names),
        )

    if skipped_names:
        fields += _fields_for(
            f"⏭️ Skipped, name already exists ({len(skipped_names)})",
            _lines_names(skipped_names),
        )

    summary = (
        f"Project: {pair_name}. "
        + _summary_part(
            "Downloaded",
            retrieved_names,
            lambda ns: ns,
        )
        + "; "
        + _summary_part(
            "Updated",
            updated_names,
            lambda ns: ns,
        )
        + "; "
        + _summary_part(
            "Skipped",
            skipped_names,
            lambda ns: ns,
        )
    )

    flavor = gemini_flavor.generate_flavor_text(
        gemini_api_key,
        summary,
        gemini_model,
        gemini_persona,
    )

    embeds = _split_into_embeds(
        f"⬇️ Retrieve result — {pair_name}",
        0x57F287,
        fields,
        flavor,
    )

    _send_embeds(webhook_url, embeds)
