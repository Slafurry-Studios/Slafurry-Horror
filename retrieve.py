#!/usr/bin/env python3
"""
retrieve.py — ONLY downloads files from Google Drive into a destination folder, and ALWAYS
reports the result to a Discord webhook (webhook is required, not optional).
Does not detect/announce "changes" the way track.py does. Can run standalone without track.py
ever having run first (it lists Drive itself).

Rules:
- A brand-new Drive file (never seen before) whose full RELATIVE PATH (subfolder + name) already
  exists in the destination folder -> SKIPPED (not overwritten). Files with the same filename but
  in different Drive subfolders are NOT considered a conflict — e.g. "player/idle.png" and
  "enemy/idle.png" both get downloaded, keeping their subfolder structure.
- A Drive file that was already downloaded before, and has since been UPDATED in Drive (same
  file id, newer modifiedTime) -> RE-DOWNLOADED, overwriting the exact same local file. This is
  not a name conflict, it's the same file getting refreshed.
- A file that's unchanged since last download -> not downloaded again.
- A file flagged deleted_in_drive -> not downloaded.
- Every run that downloads, updates, and/or skips at least one file sends a Discord notification.

Example usage:
    python retrieve.py \
        --drive-folder-id 1AbC... \
        --service-account-file sa.json \
        --state-file state/manifest.json \
        --download-dir downloaded \
        --project-name "My Project" \
        --discord-webhook https://discord.com/api/webhooks/...

Gemini flavor text (--gemini-api-key / --gemini-model / --gemini-persona) can also be set purely
via env vars GEMINI_API_KEY / GEMINI_MODEL / GEMINI_PERSONA — handy for GitHub Actions, since you
can just set them under `env:` on the step instead of building the flag manually. Explicit CLI
flags, if given, win over the env vars.
"""

import argparse
import os
import sys
from pathlib import Path

from core import discord_notifier, drive_client, state


def run_retrieve(
    drive_folder_id, sa_file, sa_b64, state_file, download_dir, project_name, webhook_url,
    gemini_api_key=None, gemini_model=None, gemini_persona=None,
):
    sa_info = drive_client.load_service_account_info(sa_file=sa_file, sa_b64=sa_b64)
    service = drive_client.build_service(sa_info)

    current = drive_client.list_files_recursive(service, drive_folder_id)
    manifest = state.load_manifest(state_file)

    # Merge the latest listing into the manifest without touching any existing retrieve_status /
    # downloaded_as / downloaded_modified_time (so retrieve.py can run standalone even if
    # track.py has never been called).
    _PRESERVE = ("retrieve_status", "downloaded_as", "downloaded_modified_time")
    for file_id, meta in current.items():
        old = manifest.get(file_id, {})
        new_entry = dict(meta)
        for key in _PRESERVE:
            if key in old:
                new_entry[key] = old[key]
        manifest[file_id] = new_entry

    download_path = Path(download_dir)
    download_path.mkdir(parents=True, exist_ok=True)
    # Keyed by RELATIVE PATH (subfolder + filename), not just the bare filename, so files with
    # the same name in different Drive subfolders don't collide with each other.
    existing_paths = {
        p.relative_to(download_path).as_posix() for p in download_path.rglob("*") if p.is_file()
    }

    retrieved, updated, skipped = [], [], []

    for fid, meta in manifest.items():
        if fid not in current:
            continue  # no longer in Drive, don't touch it here (track.py handles that report)
        if meta.get("status") == "deleted_in_drive":
            continue

        retrieve_status = meta.get("retrieve_status")

        if retrieve_status is None:
            # Brand-new file we've never processed before.
            rel_path = meta["relative_path"]
            if rel_path in existing_paths:
                # Genuine conflict: same subfolder + same filename as some OTHER file already there.
                manifest[fid]["retrieve_status"] = "skipped_name_conflict"
                skipped.append(rel_path)
                continue

            dest = drive_client.download_file(service, fid, meta["mimeType"], download_path / rel_path)
            downloaded_as = dest.relative_to(download_path).as_posix()
            manifest[fid]["retrieve_status"] = "downloaded"
            manifest[fid]["downloaded_as"] = downloaded_as
            manifest[fid]["downloaded_modified_time"] = meta.get("modifiedTime")
            existing_paths.add(downloaded_as)
            retrieved.append(downloaded_as)

        elif retrieve_status == "downloaded":
            # Already downloaded before -- has this SAME Drive file changed since then?
            if meta.get("modifiedTime") != meta.get("downloaded_modified_time"):
                local_rel_path = meta.get("downloaded_as", meta["relative_path"])
                dest = drive_client.download_file(
                    service, fid, meta["mimeType"], download_path / local_rel_path
                )
                downloaded_as = dest.relative_to(download_path).as_posix()
                manifest[fid]["downloaded_as"] = downloaded_as
                manifest[fid]["downloaded_modified_time"] = meta.get("modifiedTime")
                updated.append(downloaded_as)
            # else: unchanged, nothing to do

        # retrieve_status == "skipped_name_conflict" -> left alone, see README for how to retry

    state.save_manifest(state_file, manifest)

    # Discord notification is always attempted here (webhook is a required argument).
    discord_notifier.send_retrieve_notification(
        webhook_url, project_name, retrieved, updated, skipped,
        gemini_api_key=gemini_api_key, gemini_model=gemini_model, gemini_persona=gemini_persona,
    )

    return retrieved, updated, skipped


def main():
    parser = argparse.ArgumentParser(description="Download new files from Google Drive to a local folder")
    parser.add_argument("--drive-folder-id", required=True)
    parser.add_argument("--service-account-file")
    parser.add_argument("--service-account-b64")
    parser.add_argument("--state-file", required=True)
    parser.add_argument("--download-dir", required=True)
    parser.add_argument("--project-name", default="Drive")
    parser.add_argument("--discord-webhook", required=True, help="Required — you'll be notified of every retrieve result")
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

    retrieved, updated, skipped = run_retrieve(
        args.drive_folder_id,
        args.service_account_file,
        args.service_account_b64,
        args.state_file,
        args.download_dir,
        args.project_name,
        args.discord_webhook,
        args.gemini_api_key,
        args.gemini_model,
        args.gemini_persona,
    )

    print(f"[{args.project_name}] Downloaded: {len(retrieved)}, Updated: {len(updated)}, Skipped (name conflict): {len(skipped)}")


if __name__ == "__main__":
    main()
