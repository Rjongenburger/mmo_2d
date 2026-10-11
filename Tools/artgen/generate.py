"""Regenerates the game's art and sound into Assets/.

    python3 Tools/artgen/generate.py [--manifest path.json] [--skip-audio]

Overwriting the PNG/WAV files in place keeps their .meta files (and therefore every
reference in scenes/prefabs) intact, so it is safe to tweak and re-run.
"""
import argparse
import json
import os

import audio
import bird
import character
import effects
import font
import hero
import house
import lab
import mage
import mountain
import ui
from pixel import sheet

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))


class Manifest:
    def __init__(self):
        self.data = {"ppu": 32, "sheets": [], "singles": [], "props": [], "font": None, "audio": []}

    def save_png(self, canvas, rel_path):
        path = os.path.join(ROOT, rel_path)
        os.makedirs(os.path.dirname(path), exist_ok=True)
        canvas.save(path)
        return rel_path

    def sheet(self, frames, cols, rel_path, names, pivot, collider=None):
        """Equally sized frames packed row-major; pivot/collider in px from a frame's bottom-left."""
        self.save_png(sheet(frames, cols), rel_path)
        self.data["sheets"].append({"path": rel_path, "cellW": frames[0].w, "cellH": frames[0].h, "names": names,
                                    "pivotX": pivot[0], "pivotY": pivot[1], "collider": collider})

    def single(self, canvas, rel_path, pivot=None, border=None, collider=None):
        pivot = pivot or (canvas.w / 2, canvas.h / 2)
        self.save_png(canvas, rel_path)
        self.data["singles"].append({"path": rel_path, "name": os.path.splitext(os.path.basename(rel_path))[0],
                                     "w": canvas.w, "h": canvas.h, "pivotX": pivot[0], "pivotY": pivot[1],
                                     "border": list(border) if border else None, "collider": collider})


def generate_lab(m):
    """The original white-lab art (kept so the lab scene can still be regenerated)."""
    names = [f"Scientist_{d}_{n}" for d in character.ROW_ORDER for n in ("idle0", "idle1", "walk0", "walk1", "walk2", "walk3")]
    m.sheet([f for d in character.ROW_ORDER for f in character.build_frames()[d]], 6,
            "Assets/Art/Characters/Scientist.png", names, character.PIVOT)
    m.sheet([lab.floor_tile(k, i) for i, k in enumerate(lab.FLOOR_KINDS)], len(lab.FLOOR_KINDS),
            "Assets/Art/Lab/Tiles/Floor.png", [f"Floor_{k}" for k in lab.FLOOR_KINDS], (16, 16))
    faces = [lab.wall_face(k) for k in lab.WALL_FACE_KINDS]
    _face_sheet(m, faces, "Assets/Art/Lab/Tiles/WallFace.png", "WallFace", lab.WALL_FACE_KINDS)
    m.sheet([lab.wall_cap(i) for i in range(16)], 4, "Assets/Art/Lab/Tiles/WallTop.png", [f"WallTop_{i}" for i in range(16)], (16, 16))
    m.sheet([lab.wall_shadow(i) for i in range(8)], 8, "Assets/Art/Lab/Tiles/WallShadow.png", [f"WallShadow_{i}" for i in range(8)], (16, 16))
    for name, (canvas, pivot, collider) in lab.props().items():
        m.single(canvas, f"Assets/Art/Lab/Props/{name}.png", pivot, collider=collider)
    for name, (frames, pivot, collider) in lab.animated_props().items():
        m.sheet(frames, len(frames), f"Assets/Art/Lab/Props/{name}.png", [f"{name}_{i}" for i in range(len(frames))], pivot, collider)


def _face_sheet(m, faces, path, prefix, kinds):
    """Two-tile-tall wall/cliff faces: sheet row 0 holds the upper halves, row 1 the lower halves."""
    m.save_png(sheet(faces, len(faces)), path)
    m.data["sheets"].append({"path": path, "cellW": 32, "cellH": 32, "pivotX": 16, "pivotY": 16, "collider": None,
                             "names": [f"{prefix}_{k}_upper" for k in kinds] + [f"{prefix}_{k}_lower" for k in kinds]})


def generate_characters(m):
    m.sheet([f for d in hero.ROW_ORDER for f in hero.build_frames()[d]], len(hero.COLUMNS),
            "Assets/Art/Characters/Hero.png", hero.frame_names(), hero.PIVOT)
    m.sheet([mage.frame(i) for i in range(len(mage.COLUMNS))], len(mage.COLUMNS),
            "Assets/Art/Characters/Mage.png", mage.frame_names(), mage.PIVOT, collider=[9, 2, 16, 8])
    m.sheet(bird.build_frames(), len(bird.COLUMNS), "Assets/Art/Characters/Crow.png", bird.frame_names(), bird.PIVOT)


def generate_mountains(m):
    d = "Assets/Art/Mountains"
    tiles = [mountain.ground_tile(i) for i in range(mountain.GROUND_VARIANTS)] + [mountain.ground_edge(i) for i in range(16)]
    names = [f"Ground_{i}" for i in range(mountain.GROUND_VARIANTS)] + [f"GroundEdge_{i}" for i in range(16)]
    m.sheet(tiles, 10, f"{d}/Tiles/Ground.png", names, (16, 16))
    _face_sheet(m, [mountain.cliff_face(k) for k in mountain.CLIFF_KINDS], f"{d}/Tiles/Cliff.png", "Cliff", mountain.CLIFF_KINDS)

    m.single(mountain.cloud_sea(), f"{d}/Background/CloudSea.png")
    m.single(mountain.cloud_wisps(), f"{d}/Background/CloudWisps.png")
    for i in range(2):
        m.single(mountain.distant_peak(i), f"{d}/Background/DistantPeak_{i}.png", (64, 0))

    p = f"{d}/Props"
    m.single(mountain.pine_tree(), f"{p}/PineTree.png", (24, 4), collider=[19, 2, 11, 8])
    m.single(mountain.boulder("big", False, 1), f"{p}/BoulderBig.png", (20, 3), collider=[3, 2, 34, 12])
    m.single(mountain.boulder("big", True, 1), f"{p}/BoulderBigSnow.png", (20, 3), collider=[3, 2, 34, 12])
    m.single(mountain.boulder("small", False, 2), f"{p}/BoulderSmall.png", (13, 2), collider=[2, 1, 22, 8])
    m.single(mountain.cairn(), f"{p}/Cairn.png", (11, 2), collider=[3, 1, 16, 7])
    for i in range(2):
        m.single(mountain.standing_stone(i), f"{p}/StandingStone_{i}.png", (8, 2), collider=[2, 1, 12, 6])
    m.single(mountain.ritual_circle(), f"{p}/RitualCircle.png")
    m.sheet([mountain.prayer_flags(f) for f in range(3)], 3, f"{p}/PrayerFlags.png", [f"PrayerFlags_{i}" for i in range(3)],
            (48, 2), collider=None)
    m.sheet([mountain.rune_slab(False), mountain.rune_slab(True)], 2, f"{p}/RuneSlab.png", ["RuneSlab_dim", "RuneSlab_glow"], (16, 11))
    for i in range(3):
        drift = mountain.snow_drift(i)
        m.single(drift, f"{p}/SnowDrift_{i}.png")
    m.sheet([mountain.grass_tuft(f, v) for v in range(3) for f in range(4)], 4, f"{p}/GrassTuft.png",
            [f"GrassTuft_{v}_{f}" for v in range(3) for f in range(4)], (6, 1))
    m.single(mountain.dead_bush(), f"{p}/DeadBush.png", (11, 1))

def generate_effects(m):
    d = "Assets/Art/Effects"
    m.sheet([effects.portal(f) for f in range(effects.PORTAL_FRAMES)], effects.PORTAL_FRAMES, f"{d}/Portal.png",
            [f"Portal_{i}" for i in range(effects.PORTAL_FRAMES)], (32, 32))
    m.sheet([effects.sparkle(f) for f in range(4)], 4, f"{d}/Sparkle.png", [f"Sparkle_{i}" for i in range(4)], (3.5, 3.5))
    m.sheet([effects.dust(f) for f in range(4)], 4, f"{d}/Dust.png", [f"Dust_{i}" for i in range(4)], (8, 2))
    m.single(effects.wood_chip(), f"{d}/WoodChip.png")
    m.single(effects.wind_streak(), f"{d}/WindStreak.png")
    m.sheet([effects.snowflake(True), effects.snowflake(False)], 2, f"{d}/Snowflake.png", ["Snowflake_big", "Snowflake_small"], (1, 1))
    m.single(effects.ember(), f"{d}/Ember.png")
    m.single(effects.zzz(), f"{d}/Zzz.png")
    m.single(effects.exclaim(), f"{d}/Exclaim.png", (5.5, 0))
    m.single(effects.hint_arrow(), f"{d}/HintArrow.png", (6.5, 0))
    m.single(effects.portal_spark(), f"{d}/PortalSpark.png")
    m.single(effects.smoke(), f"{d}/Smoke.png")
    from pixel import rgb
    m.single(effects.glow_disc(32, rgb("#ffffff"), 140), f"{d}/Glow.png")
    m.sheet([effects.fireball(f) for f in range(3)], 3, f"{d}/Fireball.png", [f"Fireball_{i}" for i in range(3)], (11, 6))
    m.sheet([effects.explosion(f) for f in range(6)], 6, f"{d}/Explosion.png", [f"Explosion_{i}" for i in range(6)], (16, 16))
    m.single(effects.feather_puff(), f"{d}/FeatherPuff.png")
    m.single(effects.white_pixel(), f"{d}/WhitePixel.png", (0, 0))


def generate_house(m):
    d = "Assets/Art/House"
    m.sheet([house.floor_plank(i) for i in range(3)], 3, f"{d}/Tiles/Floor.png", [f"HouseFloor_{i}" for i in range(3)], (16, 16))
    _face_sheet(m, [house.wall_face(k) for k in ("plain", "post")], f"{d}/Tiles/WallFace.png", "HouseWall", ("plain", "post"))
    m.sheet([house.wall_cap(i) for i in range(16)], 4, f"{d}/Tiles/WallTop.png", [f"HouseWallTop_{i}" for i in range(16)], (16, 16))
    m.sheet([house.wall_shadow(i) for i in range(8)], 8, f"{d}/Tiles/WallShadow.png", [f"HouseWallShadow_{i}" for i in range(8)], (16, 16))
    p = f"{d}/Props"
    m.sheet([house.bed(s) for s in ("sleeping", "startled", "empty")], 3, f"{p}/Bed.png",
            ["Bed_sleeping", "Bed_startled", "Bed_empty"], (16, 2), collider=[1, 2, 30, 58])
    m.single(house.nightstand(), f"{p}/Nightstand.png", (10, 1), collider=[1, 1, 18, 10])
    m.sheet([house.candle_flame(f) for f in range(2)], 2, f"{p}/CandleFlame.png", ["CandleFlame_0", "CandleFlame_1"], (2.5, 0))
    m.single(house.window(), f"{p}/Window.png", (18, 0))
    m.sheet([house.fireplace(f) for f in range(3)], 3, f"{p}/Fireplace.png", [f"Fireplace_{i}" for i in range(3)], (24, 0))
    m.single(house.table(), f"{p}/Table.png", (24, 2), collider=[2, 2, 44, 12])
    m.single(house.chair(), f"{p}/Chair.png", (10, 1))
    m.single(house.chest(), f"{p}/Chest.png", (16, 2), collider=[2, 2, 28, 10])
    m.single(house.rug(), f"{p}/Rug.png")
    m.single(house.shelf(), f"{p}/Shelf.png", (24, 0))
    m.single(house.door(), f"{p}/Door.png", (16, 0))
    m.single(house.boots(), f"{p}/Boots.png", (8, 1))


def generate_ui(m):
    d = "Assets/Art/UI"
    m.single(ui.stone_panel(), f"{d}/StonePanel.png", border=(7, 7, 7, 7))
    m.single(ui.parchment_panel(), f"{d}/ParchmentPanel.png", border=(7, 7, 7, 7))
    m.single(ui.menu_panel(), f"{d}/MenuPanel.png", border=(2, 2, 2, 2))
    m.single(ui.menu_header(), f"{d}/MenuHeader.png", border=(2, 2, 2, 2))
    m.single(ui.tooltip_panel(), f"{d}/TooltipPanel.png", border=(2, 2, 2, 2))
    m.single(ui.dark_panel(), f"{d}/DarkPanel.png", border=(3, 3, 3, 3))
    m.single(ui.slot_hover(), f"{d}/SlotHover.png", border=(2, 2, 2, 2))
    m.single(ui.tab(False), f"{d}/TabNormal.png", border=(3, 3, 3, 3))
    m.single(ui.tab(True), f"{d}/TabActive.png", border=(3, 3, 3, 3))
    m.single(ui.icon_backpack(), f"{d}/IconBackpack.png")
    m.single(ui.icon_stats(), f"{d}/IconStats.png")
    m.single(ui.icon_equipment(), f"{d}/IconEquipment.png")
    m.single(ui.icon_spellbook(), f"{d}/IconSpellbook.png")
    m.single(ui.icon_magic_skill(), f"{d}/IconMagic.png")
    m.single(ui.dialogue_panel(), f"{d}/DialoguePanel.png", border=(8, 8, 8, 8))
    m.single(ui.name_plate(), f"{d}/NamePlate.png", border=(5, 5, 5, 5))
    m.single(ui.portrait_ring(), f"{d}/PortraitRing.png", border=(4, 4, 4, 4))
    m.single(ui.portrait_backdrop(), f"{d}/PortraitBackdrop.png")
    m.single(effects.continue_arrow(), f"{d}/ContinueArrow.png")
    m.sheet([effects.heart(s) for s in ("full", "half", "empty")], 3, f"{d}/Heart.png", ["Heart_full", "Heart_half", "Heart_empty"], (4.5, 4.5))
    m.single(ui.equip_slot_frame(), f"{d}/EquipSlot.png", border=(4, 4, 4, 4))
    for slot in ui.SLOT_SILHOUETTES:
        m.single(ui.slot_silhouette(slot), f"{d}/Slots/Slot{slot}.png")
    for kind in ("FireBolt", "Gust", "Frost", "Mend"):
        m.single(ui.spell_icon(kind), f"{d}/Spells/Spell{kind}.png")
        m.single(ui.spell_icon(kind, locked=True), f"{d}/Spells/Spell{kind}Locked.png")
    m.single(ui.spell_frame(True), f"{d}/SpellFrameActive.png")
    m.single(ui.spell_frame(False), f"{d}/SpellFrame.png")
    m.single(ui.keycap(), f"{d}/Keycap.png")
    m.single(ui.portrait_frame(), f"{d}/PortraitFrame.png", border=(3, 3, 3, 3))
    m.single(ui.skill_tile(), f"{d}/SkillTile.png", border=(4, 4, 4, 4))
    m.single(hero.head_portrait(), f"{d}/PortraitHero.png")
    m.single(mage.head_portrait(), f"{d}/PortraitMage.png")
    for name, fn in ui.ITEM_ICONS.items():
        m.single(fn(), f"Assets/Art/Items/{name}.png", (12, 4))

    atlas, glyphs = font.build_atlas()
    path = m.save_png(atlas, f"{d}/PixelFont.png")
    m.data["font"] = {"path": path, "glyphHeight": font.GLYPH_H, "lineHeight": font.LINE_HEIGHT,
                      "spaceAdvance": font.SPACE_ADVANCE, "letterSpacing": font.LETTER_SPACING, "glyphs": glyphs}


def generate_audio(m):
    os.makedirs(os.path.join(ROOT, "Assets", "Audio"), exist_ok=True)
    for name, fn in audio.ALL.items():
        rel = f"Assets/Audio/{name}.wav"
        fn(os.path.join(ROOT, rel))
        m.data["audio"].append(rel)


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--manifest", help="write slicing/pivot/collider info as JSON")
    ap.add_argument("--skip-audio", action="store_true")
    args = ap.parse_args()
    m = Manifest()
    generate_lab(m)
    generate_characters(m)
    generate_mountains(m)
    generate_effects(m)
    generate_house(m)
    generate_ui(m)
    if not args.skip_audio:
        generate_audio(m)
    if args.manifest:
        with open(args.manifest, "w") as f:
            json.dump(m.data, f, indent=1)
    print(f"Wrote {len(m.data['sheets'])} sheets, {len(m.data['singles'])} single sprites, "
          f"{len(m.data['audio'])} sounds under {ROOT}/Assets")


if __name__ == "__main__":
    main()
