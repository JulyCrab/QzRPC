#!/usr/bin/env python3
"""
Build a macOS .icns icon from a single square PNG, on any platform.

This replaces the macOS-only `sips`/`iconutil` toolchain so the icon can be
(re)generated and verified on Windows/Linux too. It writes the same set of
icon variants that `iconutil` produces from a standard .iconset.

Usage:
    python build/macos/png_to_icns.py Assets/musical-notes.png build/macos/AppIcon.icns

The generated AppIcon.icns is committed to the repo; the .app bundle script
just copies it. Re-run this only when the source artwork changes. For crisp
Retina icons, feed a 1024x1024 source (smaller sources are upscaled).

Requires: Pillow  (pip install pillow)
"""
import io
import struct
import sys

from PIL import Image

# OSType -> pixel size, mirroring iconutil's .iconset -> .icns mapping.
# (icp4/icp5 = base 16/32; ic11..ic14 = the @2x Retina variants.)
SPEC = [
    (b"icp4", 16),    # icon_16x16
    (b"ic11", 32),    # icon_16x16@2x
    (b"icp5", 32),    # icon_32x32
    (b"ic12", 64),    # icon_32x32@2x
    (b"ic07", 128),   # icon_128x128
    (b"ic13", 256),   # icon_128x128@2x
    (b"ic08", 256),   # icon_256x256
    (b"ic14", 512),   # icon_256x256@2x
    (b"ic09", 512),   # icon_512x512
    (b"ic10", 1024),  # icon_512x512@2x
]


def build_icns(src_path: str, out_path: str) -> None:
    src = Image.open(src_path).convert("RGBA")

    png_by_size: dict[int, bytes] = {}
    chunks = bytearray()
    for ostype, size in SPEC:
        if size not in png_by_size:
            img = src.resize((size, size), Image.LANCZOS)
            buf = io.BytesIO()
            img.save(buf, format="PNG")
            png_by_size[size] = buf.getvalue()
        png = png_by_size[size]
        chunks += ostype + struct.pack(">I", 8 + len(png)) + png

    total = 8 + len(chunks)
    with open(out_path, "wb") as f:
        f.write(b"icns" + struct.pack(">I", total) + chunks)

    print(f"Wrote {out_path} ({total} bytes, {len(SPEC)} variants)")


if __name__ == "__main__":
    if len(sys.argv) != 3:
        sys.exit("usage: png_to_icns.py <input.png> <output.icns>")
    build_icns(sys.argv[1], sys.argv[2])
