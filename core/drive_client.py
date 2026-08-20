"""Thin wrapper around the Google Drive API. Shared by track.py & retrieve.py."""

import base64
import json
from pathlib import Path

from google.oauth2 import service_account
from googleapiclient.discovery import build

SCOPES = ["https://www.googleapis.com/auth/drive.readonly"]


def load_service_account_info(sa_file=None, sa_b64=None):
    """Accepts either: a path to a service account JSON file, or a base64 string of its content."""
    if sa_file:
        return json.loads(Path(sa_file).read_text())
    if sa_b64:
        return json.loads(base64.b64decode(sa_b64))
    raise ValueError("Need either sa_file or sa_b64")


def build_service(sa_info: dict):
    creds = service_account.Credentials.from_service_account_info(sa_info, scopes=SCOPES)
    return build("drive", "v3", credentials=creds)


def list_files_recursive(service, folder_id, path_prefix=""):
    """List all non-folder (non-trashed) files inside folder_id, including subfolders.
    Returns a dict {file_id: metadata}."""
    files = {}
    page_token = None
    while True:
        resp = (
            service.files()
            .list(
                q=f"'{folder_id}' in parents and trashed = false",
                spaces="drive",
                fields=(
                    "nextPageToken, files(id, name, mimeType, modifiedTime, "
                    "webViewLink, size)"
                ),
                pageToken=page_token,
                pageSize=1000,
            )
            .execute()
        )
        for f in resp.get("files", []):
            if f["mimeType"] == "application/vnd.google-apps.folder":
                files.update(list_files_recursive(service, f["id"], path_prefix + f["name"] + "/"))
            else:
                f["relative_path"] = path_prefix + f["name"]
                files[f["id"]] = f
        page_token = resp.get("nextPageToken")
        if not page_token:
            break
    return files


_EXPORT_MAP = {
    "application/vnd.google-apps.document": (
        "application/pdf", ".pdf",
    ),
    "application/vnd.google-apps.spreadsheet": (
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", ".xlsx",
    ),
    "application/vnd.google-apps.presentation": (
        "application/vnd.openxmlformats-officedocument.presentationml.presentation", ".pptx",
    ),
}


def download_file(service, file_id, mime_type, dest_path: Path) -> Path:
    """Download (or export, for native Google Docs/Sheets/Slides) to dest_path.
    Returns the final path actually used (extension may change for native files)."""
    dest_path.parent.mkdir(parents=True, exist_ok=True)

    if mime_type in _EXPORT_MAP:
        export_mime, ext = _EXPORT_MAP[mime_type]
        if dest_path.suffix.lower() != ext:
            dest_path = dest_path.with_suffix(ext)
        request = service.files().export_media(fileId=file_id, mimeType=export_mime)
    else:
        request = service.files().get_media(fileId=file_id)

    data = request.execute()
    dest_path.write_bytes(data)
    return dest_path
