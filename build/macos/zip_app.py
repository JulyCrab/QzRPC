#!/usr/bin/env python3
"""
Zip a macOS .app bundle while preserving the Unix executable bit in the archive
metadata (create_system = Unix, external_attr = mode << 16).

This lets a bundle built on Windows still launch on macOS: NTFS can't store the
executable bit, but the bit can ride along inside the zip and be restored when
macOS (Finder's Archive Utility or `unzip`) extracts it.

Usage:
    python build/macos/zip_app.py <path-to.app> <output.zip>
"""
import os
import sys
import zipfile


def zip_app(app_dir: str, out_zip: str) -> None:
    app_dir = app_dir.rstrip("/\\")
    parent = os.path.dirname(app_dir)  # zip paths are relative to this (keepParent)

    with zipfile.ZipFile(out_zip, "w", zipfile.ZIP_DEFLATED) as z:
        for root, _dirs, files in os.walk(app_dir):
            for name in files:
                full = os.path.join(root, name)
                rel = os.path.relpath(full, parent).replace(os.sep, "/")

                # Everything under Contents/MacOS/ is an executable; rest is data.
                executable = "/Contents/MacOS/" in "/" + rel
                mode = 0o100755 if executable else 0o100644

                info = zipfile.ZipInfo(rel)
                info.create_system = 3            # Unix
                info.external_attr = mode << 16
                info.compress_type = zipfile.ZIP_DEFLATED
                with open(full, "rb") as f:
                    z.writestr(info, f.read())

    print(f"Wrote {out_zip}")


if __name__ == "__main__":
    if len(sys.argv) != 3:
        sys.exit("usage: zip_app.py <path-to.app> <output.zip>")
    zip_app(sys.argv[1], sys.argv[2])
