"""Send notifications to a Discord webhook.

Used by track.py (Drive change reports) and retrieve.py (download result reports).

Long lists are never silently truncated:
    lines -> fields -> embeds -> webhook messages

The implementation deliberately stays well below Discord's hard limits.
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

# We deliberately stay comfortably below Discord's hard limits.
SAFE_FIELD_VALUE_LIMIT = 900
SAFE_EMBED_LIMIT = 5000


# ---------------------------------------------------------------------------
# Discord character counting
# ---------------------------------------------------------------------------

def _discord_len(value):
    """Count UTF-16 code units.

    Discord's character limits are based on Unicode/UTF-16 semantics rather
    than Python's simple len() behavior. This matters for emoji and some
    non-BMP Unicode characters.
    """
    if value is None:
        return 0

    return len(str(value).encode("utf-16-le")) // 2


# ---------------------------------------------------------------------------
# Basic formatting
# ---------------------------------------------------------------------------

def _safe_title(title):
    """Keep a title safely below Discord's title limit."""
    title = str(title)

    if _discord_len(title) <= TITLE_LIMIT:
        return title

    result = ""

    for char in title:
        candidate = result + char + "…"

        if _discord_len(candidate) > TITLE_LIMIT:
            break

        result += char

    return result + "…"


def _lines_files(files):
    """Convert Drive file dictionaries into Discord markdown lines."""
    result = []

    for f in files:
        path = str(f.get("relative_path", ""))
        link = str(f.get("webViewLink", ""))

        if link:
            result.append(f"- [{path}]({link})")
        else:
            result.append(f"- {path}")

    return result


def _lines_names(names):
    """Convert plain names into Discord markdown lines."""
    return [f"- {name}" for name in names]


# ---------------------------------------------------------------------------
# Gemini helpers
# ---------------------------------------------------------------------------

_SAMPLE_NAMES_LIMIT = 3
_SAMPLE_NAME_MAXLEN = 60


def _sample_names(names, limit=_SAMPLE_NAMES_LIMIT):
    """Create a short representative list for Gemini."""
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
    """Build one compact Gemini summary section."""
    if not items:
        return f"{label}: 0"

    sample = _sample_names(names_fn(items))

    return f"{label}: {len(items)} ({sample})"


# ---------------------------------------------------------------------------
# Field splitting
# ---------------------------------------------------------------------------

def _split_single_line(line, limit=SAFE_FIELD_VALUE_LIMIT):
    """Split one unusually long line without dropping any content."""
    line = str(line)

    if _discord_len(line) <= limit:
        return [line]

    chunks = []
    current = ""

    for char in line:
        candidate = current + char

        if current and _discord_len(candidate) > limit:
            chunks.append(current)
            current = char
        else:
            current = candidate

    if current:
        chunks.append(current)

    return chunks


def _chunk_by_length(lines, limit=SAFE_FIELD_VALUE_LIMIT):
    """Group lines into safe Discord field-value chunks."""
    chunks = []
    current = []
    current_len = 0

    for original_line in lines:
        for line in _split_single_line(original_line, limit):
            line_len = _discord_len(line)

            # Newline between entries.
            if current:
                line_len += 1

            if current and current_len + line_len > limit:
                chunks.append(current)
                current = []
                current_len = 0

                line_len = _discord_len(line)

            current.append(line)
            current_len += line_len

    if current:
        chunks.append(current)

    return chunks


def _fields_for(label, lines):
    """Turn lines into Discord fields."""
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

    fields = []
    total = len(chunks)

    for i, chunk in enumerate(chunks, start=1):
        if total == 1:
            name = label
        else:
            name = f"{label} ({i}/{total})"

        name = _safe_title(name)

        value = "\n".join(chunk)

        # Final defensive assertion.
        if _discord_len(value) > FIELD_VALUE_LIMIT:
            raise RuntimeError(
                "Internal error: generated Discord field exceeds "
                f"{FIELD_VALUE_LIMIT} UTF-16 characters"
            )

        fields.append(
            {
                "name": name,
                "value": value,
                "inline": False,
            }
        )

    return fields


# ---------------------------------------------------------------------------
# Embed sizing
# ---------------------------------------------------------------------------

def _embed_size(embed):
    """Calculate the Discord character count for an embed."""
    total = 0

    total += _discord_len(embed.get("title", ""))
    total += _discord_len(embed.get("description", ""))

    for field in embed.get("fields", []):
        total += _discord_len(field.get("name", ""))
        total += _discord_len(field.get("value", ""))

    author = embed.get("author")

    if author:
        total += _discord_len(author.get("name", ""))

    footer = embed.get("footer")

    if footer:
        total += _discord_len(footer.get("text", ""))

    return total


def _make_embed(title, color, fields=None, description=None):
    """Create an embed."""
    embed = {
        "title": _safe_title(title),
        "color": color,
        "fields": fields or [],
    }

    if description:
        description = str(description)

        # Keep Gemini text well below Discord's description limit.
        if _discord_len(description) > DESCRIPTION_LIMIT:
            result = ""

            for char in description:
                if _discord_len(result + char) > DESCRIPTION_LIMIT:
                    break

                result += char

            description = result

        embed["description"] = description

    return embed


# ---------------------------------------------------------------------------
# Embed splitting
# ---------------------------------------------------------------------------

def _split_into_embeds(title, color, fields, description=None):
    """Split a report into Discord-safe embeds."""

    title = _safe_title(title)

    # Gemini flavor text goes only on the first embed.
    if description:
        description = str(description)

    embeds = []

    current_fields = []
    first = True

    for field in fields:
        candidate = _make_embed(
            title=title,
            color=color,
            fields=current_fields + [field],
            description=description if first else None,
        )

        too_many_fields = (
            len(current_fields) >= MAX_FIELDS_PER_EMBED
        )

        too_many_chars = (
            _embed_size(candidate) > SAFE_EMBED_LIMIT
        )

        if current_fields and (too_many_fields or too_many_chars):
            embeds.append(
                _make_embed(
                    title=title,
                    color=color,
                    fields=current_fields,
                    description=description if first else None,
                )
            )

            current_fields = [field]
            first = False

        else:
            current_fields.append(field)

    if current_fields:
        embeds.append(
            _make_embed(
                title=title,
                color=color,
                fields=current_fields,
                description=description if first else None,
            )
        )

    if not embeds:
        embeds.append(
            _make_embed(
                title=title,
                color=color,
                description=description,
            )
        )

    # Add part numbers.
    total = len(embeds)

    if total > 1:
        numbered = []

        for i, embed in enumerate(embeds, start=1):
            numbered_title = _safe_title(
                f"{title} (part {i}/{total})"
            )

            new_embed = dict(embed)
            new_embed["title"] = numbered_title

            # Part numbering slightly increases the embed size.
            # If that somehow exceeds our safe threshold, remove the
            # description from this particular embed.
            if _embed_size(new_embed) > SAFE_EMBED_LIMIT:
                new_embed.pop("description", None)

            numbered.append(new_embed)

        embeds = numbered

    return embeds


# ---------------------------------------------------------------------------
# Emergency splitting
# ---------------------------------------------------------------------------

def _emergency_split_embed(embed):
    """Split an unexpectedly oversized embed into smaller embeds.

    This is an additional safety net. Under normal circumstances the regular
    splitter above means this function is never needed.
    """

    title = embed.get("title", "Discord notification")
    color = embed.get("color", 0)

    fields = embed.get("fields", [])

    if not fields:
        # If there are no fields, send a plain-content fallback instead.
        return [
            {
                "title": _safe_title(title),
                "color": color,
                "description": str(embed.get("description", ""))[:1000],
            }
        ]

    result = []

    for field in fields:
        value = str(field.get("value", ""))

        chunks = _split_single_line(
            value,
            limit=800,
        )

        for i, chunk in enumerate(chunks, start=1):
            name = str(field.get("name", ""))

            if len(chunks) > 1:
                name = f"{name} ({i}/{len(chunks)})"

            result.append(
                _make_embed(
                    title=title,
                    color=color,
                    fields=[
                        {
                            "name": _safe_title(name),
                            "value": chunk,
                            "inline": False,
                        }
                    ],
                )
            )

    return result


def _validate_embeds(embeds):
    """Validate every embed before it reaches Discord."""

    valid = []

    for embed in embeds:
        size = _embed_size(embed)
        field_count = len(embed.get("fields", []))

        if (
            size <= MAX_EMBED_CHARS
            and field_count <= MAX_FIELDS_PER_EMBED
        ):
            valid.append(embed)
            continue

        print(
            "WARNING: Discord embed exceeded a hard limit locally; "
            "splitting it before sending. "
            f"size={size}, fields={field_count}"
        )

        replacements = _emergency_split_embed(embed)

        for replacement in replacements:
            if (
                _embed_size(replacement) <= MAX_EMBED_CHARS
                and len(replacement.get("fields", []))
                <= MAX_FIELDS_PER_EMBED
            ):
                valid.append(replacement)
            else:
                # Absolute last resort: convert it to a tiny embed.
                # This should effectively never be reached.
                valid.append(
                    {
                        "title": "Drive notification",
                        "description": "A notification was generated but was too large to display in one embed.",
                    }
                )

    return valid


# ---------------------------------------------------------------------------
# Webhook sender
# ---------------------------------------------------------------------------

def _send_embeds(webhook_url, embeds):
    """Send embeds in batches of at most 10."""

    embeds = _validate_embeds(embeds)

    if not embeds:
        return

    for start in range(0, len(embeds), MAX_EMBEDS_PER_MESSAGE):
        batch = embeds[start:start + MAX_EMBEDS_PER_MESSAGE]

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

            # Print useful local diagnostics.
            print(
                "Discord batch diagnostics:",
                f"embeds={len(batch)}",
                f"sizes={[_embed_size(e) for e in batch]}",
                f"fields={[len(e.get('fields', [])) for e in batch]}",
            )

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
    """Report Drive changes."""

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
    """Report retrieve/download results."""

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
