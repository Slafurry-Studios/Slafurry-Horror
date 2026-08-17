"""Optional: ask Gemini for a short, lively one-liner to put in the Discord embed's
description, based on what changed. Purely cosmetic — if GEMINI_API_KEY isn't set, or the
call fails/times out for any reason, this quietly returns None and the notification still
sends normally without it. Never blocks or breaks the main track/retrieve flow.
"""

import requests

DEFAULT_MODEL = "gemini-3.1-flash-lite"
_ENDPOINT = "https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent"
_TIMEOUT_SECONDS = 8

# Used whenever no persona is supplied (no --gemini-persona flag, no GEMINI_PERSONA env var).
# Keeps the bot's personality baked-in by default, while still letting it be overridden per
# project via env/secret without touching code.
DEFAULT_PERSONA = (
    "You are a lively, funny Discord bot for a small game dev team's asset pipeline."
)


def generate_flavor_text(api_key, summary_text, model=None, persona=None):
    """summary_text: a short plain-text summary of what happened, including a few sample
    filenames per category (not the full list) — e.g.
    'Project: MyGame (sprite). New: 2 (player/idle.png, enemy/walk.png); Changed: 1
    (ui/button.png); Removed: 0'.
    persona: a short description of the bot's character/personality/voice. Falls back to
    DEFAULT_PERSONA when not provided, so the bot always keeps *some* personality even if the
    caller forgets to set one.
    Returns a short string (or None if unavailable/failed)."""
    if not api_key or not summary_text:
        return None

    persona = (persona or DEFAULT_PERSONA).strip()

    prompt = (
        f"{persona} "
        "Write ONE short, punchy sentence (max ~18 words) reacting to this Google Drive "
        "update, casual tone, playful, in Indonesian (Bahasa gaul santai). No markdown, "
        "no emoji-spam (at most one emoji), no quotes around the sentence.\n\n"
        f"Update: {summary_text}"
    )

    body = {
        "contents": [{"parts": [{"text": prompt}]}],
        "generationConfig": {"maxOutputTokens": 60},
    }

    try:
        resp = requests.post(
            _ENDPOINT.format(model=model or DEFAULT_MODEL),
            params={"key": api_key},
            json=body,
            timeout=_TIMEOUT_SECONDS,
        )
        resp.raise_for_status()
        data = resp.json()
        text = data["candidates"][0]["content"]["parts"][0]["text"].strip()
        return text or None
    except Exception:
        # Never let a Gemini hiccup break the actual notification.
        return None
