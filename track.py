#!/usr/bin/env python3
"""
track.py — ONLY detects changes in Google Drive and sends a Discord notification.
Does not download any files. Good for a scheduled run every morning.

Example usage:
    python track.py \
        --drive-folder-id 1AbC... \
        --service-account-file sa.json \
        --discord-webhook https://discord.com/api/webhooks/... \
        --state-file state/manifest.json \
        --project-name "My Project" \
        --gemini-api-key AIza...          # optional, adds a fun one-liner to the notification

The service account can be provided via --service-account-file (local path)
OR --service-account-b64 (base64 string, handy for a CI secret).

Gemini flavor text (--gemini-api-key / --gemini-model / --gemini-persona) can also be set purely
via env vars GEMINI_API_KEY / GEMINI_MODEL / GEMINI_PERSONA — handy for GitHub Actions, since you
can just set them under `env:` on the step instead of building the flag manually. Explicit CLI
flags, if given, win over the env vars.
"""

import argparse
import os
import sys

from core import discord_notifier, drive_client, state


def run_track(
    drive_folder_id, sa_file, sa_b64, webhook_url, state_file, project_name="Drive",
    gemini_api_key=None, gemini_model=None, gemini_persona=None,
):
    sa_info = drive_client.load_service_account_info(sa_file=sa_file, sa_b64=sa_b64)
    service = drive_client.build_service(sa_info)

    current = drive_client.list_files_recursive(service, drive_folder_id)
    previous = state.load_manifest(state_file)

    new_files, changed_files, deleted_files = state.diff_files(current, previous)

    discord_notifier.send_track_notification(
        webhook_url, project_name, new_files, changed_files, deleted_files,
        gemini_api_key=gemini_api_key, gemini_model=gemini_model, gemini_persona=gemini_persona,
    )

    merged = state.merge_current_into_manifest(current, previous, deleted_files)
    state.save_manifest(state_file, merged)

    return new_files, changed_files, deleted_files


def main():
    parser = argparse.ArgumentParser(description="Track Google Drive changes -> Discord")
    parser.add_argument("--drive-folder-id", required=True)
    parser.add_argument("--service-account-file")
    parser.add_argument("--service-account-b64")
    parser.add_argument("--discord-webhook", required=True)
    parser.add_argument("--state-file", required=True)
    parser.add_argument("--project-name", default="Drive")
    parser.add_argument(
        "--gemini-api-key", default=os.environ.get("GEMINI_API_KEY"),
        help="Optional. Adds an AI one-liner to the Discord message. Defaults to env GEMINI_API_KEY.",
    )
    parser.add_argument(
        "--gemini-model", default=os.environ.get("GEMINI_MODEL"),
        help="Optional. Defaults to env GEMINI_MODEL, or gemini-3.1-flash-lite if that's unset.",
    )
    parser.add_argument(
        "--gemini-persona", default=os.environ.get("GEMINI_PERSONA"),
        help="Optional. Describes the bot's character/personality/voice for the AI one-liner. "
             "Defaults to env GEMINI_PERSONA, or a built-in default persona if that's unset too.",
    )
    args = parser.parse_args()

    if not args.service_account_file and not args.service_account_b64:
        sys.exit("ERROR: need either --service-account-file or --service-account-b64")

    new_files, changed_files, deleted_files = run_track(
        args.drive_folder_id,
        args.service_account_file,
        args.service_account_b64,
        args.discord_webhook,
        args.state_file,
        args.project_name,
        args.gemini_api_key,
        args.gemini_model,
        args.gemini_persona,
    )

    print(f"[{args.project_name}] New: {len(new_files)}, Changed: {len(changed_files)}, Deleted: {len(deleted_files)}")


if __name__ == "__main__":
    main()
