"""Regenerates the lab art into Assets/Art.

    python3 Tools/artgen/generate.py [--manifest path.json]

Overwriting the PNGs in place keeps their .meta files (and therefore every
reference in scenes/prefabs) intact, so it is safe to tweak and re-run.
"""
import argparse
import json
import os

import character
import lab
from pixel import Canvas, sheet

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
ART = os.path.join(ROOT, "Assets", "Art")


def _save(canvas, *parts):
    path = os.path.join(ART, *parts)
    os.makedirs(os.path.dirname(path), exist_ok=True)
    canvas.save(path)
    return "Assets/Art/" + "/".join(parts)


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--manifest", help="write slicing/pivot/collider info as JSON")
    args = ap.parse_args()
    manifest = {"ppu": lab.T, "sheets": [], "props": []}

    def add_sheet(path, cell_w, cell_h, names, pivot):
        manifest["sheets"].append({"path": path, "cellW": cell_w, "cellH": cell_h, "names": names,
                                   "pivotX": pivot[0], "pivotY": pivot[1]})

    # character
    path = _save(character.build_sheet(), "Characters", "Scientist.png")
    names = [f"Scientist_{d}_{n}" for d in character.ROW_ORDER
             for n in ("idle0", "idle1", "walk0", "walk1", "walk2", "walk3")]
    add_sheet(path, character.FRAME_W, character.FRAME_H, names, character.PIVOT)

    # tiles
    path = _save(sheet([lab.floor_tile(k, i) for i, k in enumerate(lab.FLOOR_KINDS)], len(lab.FLOOR_KINDS)), "Lab", "Tiles", "Floor.png")
    add_sheet(path, 32, 32, [f"Floor_{k}" for k in lab.FLOOR_KINDS], (16, 16))

    faces = [lab.wall_face(k) for k in lab.WALL_FACE_KINDS]
    path = _save(sheet(faces, len(faces)), "Lab", "Tiles", "WallFace.png")
    # sheet() packs row-major and each face is 2 cells tall: top row = upper halves, bottom row = lower halves
    add_sheet(path, 32, 32, [f"WallFace_{k}_upper" for k in lab.WALL_FACE_KINDS] +
              [f"WallFace_{k}_lower" for k in lab.WALL_FACE_KINDS], (16, 16))

    path = _save(sheet([lab.wall_cap(m) for m in range(16)], 4), "Lab", "Tiles", "WallTop.png")
    add_sheet(path, 32, 32, [f"WallTop_{m}" for m in range(16)], (16, 16))

    path = _save(sheet([lab.wall_shadow(m) for m in range(8)], 8), "Lab", "Tiles", "WallShadow.png")
    add_sheet(path, 32, 32, [f"WallShadow_{m}" for m in range(8)], (16, 16))

    # props
    for name, (canvas, pivot, collider) in lab.props().items():
        path = _save(canvas, "Lab", "Props", f"{name}.png")
        manifest["props"].append({"name": name, "path": path, "frames": 1, "frameW": canvas.w, "frameH": canvas.h,
                                  "pivotX": pivot[0], "pivotY": pivot[1], "collider": collider})
    for name, (frames, pivot, collider) in lab.animated_props().items():
        path = _save(sheet(frames, len(frames)), "Lab", "Props", f"{name}.png")
        f0 = frames[0]
        add_sheet(path, f0.w, f0.h, [f"{name}_{i}" for i in range(len(frames))], pivot)
        manifest["props"].append({"name": name, "path": path, "frames": len(frames), "frameW": f0.w, "frameH": f0.h,
                                  "pivotX": pivot[0], "pivotY": pivot[1], "collider": collider})

    if args.manifest:
        with open(args.manifest, "w") as f:
            json.dump(manifest, f, indent=1)
    print(f"Wrote {len(manifest['sheets'])} sheets and {len(manifest['props'])} props to {ART}")


if __name__ == "__main__":
    main()
