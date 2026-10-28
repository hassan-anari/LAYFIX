"""Verify original blobs in the dedicated import commit, plus unchanged dataset/assets."""
import hashlib
import json
import pathlib
import subprocess

root = pathlib.Path(__file__).resolve().parent.parent
manifest = json.loads((root / "provenance/upstream-import.json").read_text())
assert manifest["commit"] == "969814d2dbb6819eaef984525ef11db4c3f48dca"
import_commit = (root / "provenance/import-commit.txt").read_text().strip()
for entry in manifest["files"]:
    original = subprocess.check_output(["git", "show", f"{import_commit}:{entry['path']}"], cwd=root)
    assert len(original) == entry["bytes"], entry["path"]
    assert hashlib.sha256(original).hexdigest() == entry["sha256"], entry["path"]
    assert hashlib.sha1(b"blob " + str(len(original)).encode() + b"\0" + original).hexdigest() == entry["git_blob"]
    if not entry["path"].startswith("windows/src/") and entry["path"] not in ("windows/LayoutFixer.Windows.csproj", "windows/README.md"):
        assert (root / entry["path"]).read_bytes() == original, entry["path"]
print(f"Verified {len(manifest['files'])} upstream blobs against import commit {import_commit}")
