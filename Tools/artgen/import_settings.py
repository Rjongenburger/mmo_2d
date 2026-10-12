"""Applies a generate.py manifest to the art's .meta files, so Unity imports what was just generated.

    python3 Tools/artgen/generate.py --manifest /tmp/art.json
    python3 Tools/artgen/import_settings.py /tmp/art.json

Sets pixels-per-unit, bilinear filtering, the max texture size, single-sprite 9-slice borders and every
sheet sprite's rect (matched by sprite name). Sprite names, GUIDs and internal IDs are left alone, so
scenes and prefabs keep their references. Safe to re-run.
"""
import json
import os
import re
import sys

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
FONT_ASSET = "Assets/Data/UI/PixelFont.asset"
MAX_TEXTURE_SIZE = 4096


def _sub(pattern, repl, text, path, count=0):
    out, n = re.subn(pattern, repl, text, count=count, flags=re.M)
    if not n:
        raise SystemExit(f"{path}: no match for {pattern!r}")
    return out


def _texture_settings(text, path, ppu=None):
    text = _sub(r"^(    filterMode: )-?\d+", r"\g<1>1", text, path)
    text = _sub(r"^(\s+maxTextureSize: )\d+", rf"\g<1>{MAX_TEXTURE_SIZE}", text, path)
    if ppu is not None:
        text = _sub(r"^(  spritePixelsToUnits: )\S+", rf"\g<1>{ppu}", text, path)
    return text


def _sprite_rects(text, path, names, rects):
    wanted = dict(zip(names, rects))
    blocks = re.split(r"(?=^    - serializedVersion: 2\n      name: )", text, flags=re.M)
    seen = set()
    for i, block in enumerate(blocks):
        m = re.match(r"    - serializedVersion: 2\n      name: (\S+)\n", block)
        if not m or m.group(1) not in wanted:
            continue
        x, y, w, h = wanted[m.group(1)]
        blocks[i] = re.sub(r"(      rect:\n        serializedVersion: 2\n)        x: \S+\n        y: \S+\n"
                           r"        width: \S+\n        height: \S+\n",
                           rf"\g<1>        x: {x}\n        y: {y}\n        width: {w}\n        height: {h}\n", block, count=1)
        seen.add(m.group(1))
    missing = set(names) - seen
    if missing:
        raise SystemExit(f"{path}: sprites missing from .meta: {sorted(missing)}")
    return "".join(blocks)


def _patch(rel_path, fn):
    meta = os.path.join(ROOT, rel_path + ".meta")
    with open(meta) as f:
        text = f.read()
    new = fn(text, meta)
    if new != text:
        with open(meta, "w") as f:
            f.write(new)


def main():
    with open(sys.argv[1]) as f:
        manifest = json.load(f)
    ppu = manifest["ppu"]

    for s in manifest["sheets"]:
        _patch(s["path"], lambda t, p, s=s: _sprite_rects(_texture_settings(t, p, ppu), p, s["names"], s["rects"]))

    for s in manifest["singles"]:
        def single(t, p, s=s):
            t = _texture_settings(t, p, ppu)
            if s["border"]:
                left, bottom, right, top = s["border"]
                t = _sub(r"^(  spriteBorder: ).*$", rf"\g<1>{{x: {left}, y: {bottom}, z: {right}, w: {top}}}", t, p)
            return t
        _patch(s["path"], single)

    font = manifest["font"]
    if font:
        # The font atlas is a plain texture sampled by PixelText, not a sprite.
        _patch(font["path"], _texture_settings)
        asset = os.path.join(ROOT, FONT_ASSET)
        with open(asset) as f:
            text = f.read()
        if re.search(r"^  atlasScale: ", text, re.M):
            text = _sub(r"^(  atlasScale: )\d+", rf"\g<1>{font['atlasScale']}", text, asset)
        else:
            text = _sub(r"^(  letterSpacing: .*\n)", rf"\g<1>  atlasScale: {font['atlasScale']}\n", text, asset, count=1)
        with open(asset, "w") as f:
            f.write(text)

    print(f"Updated import settings for {len(manifest['sheets'])} sheets and {len(manifest['singles'])} sprites (ppu {ppu})")


if __name__ == "__main__":
    main()
