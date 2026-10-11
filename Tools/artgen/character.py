"""Scientist player character: 32x48 frames, 4 directions x (2 idle + 4 walk)."""
from pixel import Canvas, rgb

FRAME_W, FRAME_H = 32, 48
# Pivot (feet) in pixels from the bottom-left of a frame; Unity importer uses this.
PIVOT = (16, 2)

PAL = {
    "o": rgb("#2b2f3d"),  # outline
    "H": rgb("#e0803a"),  # hair
    "h": rgb("#f6aa62"),  # hair highlight
    "d": rgb("#a5512a"),  # hair shade
    "S": rgb("#f8d4b6"),  # skin
    "s": rgb("#e3a988"),  # skin shade
    "r": rgb("#f3a39a"),  # blush
    "e": rgb("#2b2f3d"),  # eyes
    "w": rgb("#ffffff"),  # eye glint
    "m": rgb("#b8574f"),  # mouth
    "C": rgb("#f7f9fc"),  # coat
    "c": rgb("#d3dbe6"),  # coat shade
    "k": rgb("#a7b3c4"),  # coat deep shade
    "T": rgb("#2bb3a8"),  # shirt
    "t": rgb("#1b7f7a"),  # shirt shade
    "Y": rgb("#f2c94c"),  # id badge
    "y": rgb("#c99a1e"),  # badge shade
    "P": rgb("#43507a"),  # trousers
    "p": rgb("#2f3858"),  # trousers shade
    "B": rgb("#323746"),  # shoes
    "b": rgb("#5a6479"),  # shoe highlight
}

HEAD_DOWN = [
    "....oooooooo....",
    "..ooHHhhHHHHoo..",
    ".oHHhhhHHHHHHHo.",
    ".oHhhHHHHHHHHdo.",
    "oHHhHHHHHHHHHHdo",
    "oHHHHHHHHHHHHddo",
    "oHHHHHHHdHHHHHdo",
    "oHHHHdHHSdHHHHdo",
    "oHHdSSdSSSSdHHdo",
    "odSSSSSSSSSSSHdo",
    "odSSeeSSSSeeSSdo",
    "odSSewSSSSewSSdo",
    ".oSrSSSSSSSSrSo.",
    ".osSSSSSmmSSSso.",
    "..oosSSSSSSsoo..",
    "....oooooooo....",
]

HEAD_UP = [
    "....oooooooo....",
    "..ooHHHHHHHHoo..",
    ".oHHHHHhhHHHHHo.",
    ".oHHHHhhHHHHHHo.",
    "oHHHHHhHHHHHHHHo",
    "oHHHHHHHHHHHHHdo",
    "oHHHHHHHHHHHHHdo",
    "oHHHHHHHHHHHHHdo",
    "odHHHHHHHHHHHHdo",
    "odHHHHHHHHHHHddo",
    "odHHHHHHHHHHHddo",
    "oddHHHHHHHHHdddo",
    ".oddHHdHHdHHddo.",
    ".osddddddddddso.",
    "..oosSSSSSSsoo..",
    "....oooooooo....",
]

HEAD_RIGHT = [
    "....ooooooo.....",
    "..ooHHHhhHHoo...",
    ".oHHHHhhhHHHHo..",
    ".oHHHHHhHHHHHHo.",
    "oHHHHHHHHHHHHHHo",
    "odHHHHHHHHHHHHHo",
    "odHHHHHHHHHHdHHo",
    "odHHHHHHHHHdSSdo",
    "odHHHHHHHHdSSSSo",
    "odHHHHHHdsSSSSSo",
    "odHHHHHdsSSSSeeo",
    "odHHHHHdsSSSSewo",
    ".odHHHHdsSSSSSro",
    ".oddHHHdsSSSSmo.",
    "..oooddosSSSSo..",
    "......oooooo....",
]

TORSO_DOWN = [
    "....osSSso....",
    "..oocTTTTcoo..",
    ".oCCCcTTcCCCo.",
    ".oCCCCcTcCCCo.",
    ".oCCCCCcCYyCo.",
    ".oCCCCCcCyyCo.",
    ".oCCCCCcCCCCo.",
    ".ocCCCCcCCCco.",
    ".ocCCCCcCCCco.",
    ".ocCCCCcCCCco.",
    ".okccccokcccko",
    "..oooooooooooo",
]

TORSO_UP = [
    "....osSSso....",
    "..ooCCCCCCoo..",
    ".oCCCCCCCCCCo.",
    ".oCCCCCCCCCCo.",
    ".oCcCCCCCCcCo.",
    ".oCcCCCCCCcCo.",
    ".oCCCCCCCCCCo.",
    ".ocCCCCCCCCco.",
    ".ocCCCCcCCCco.",
    ".ocCCCCcCCCco.",
    ".okcccckccccko",
    "..oooooooooooo",
]

TORSO_RIGHT = [
    "....osSso.....",
    "...ocCCTTo....",
    "..oCCCCCTTo...",
    "..oCCCCCcTo...",
    "..okCCCCCcYo..",
    "..okCCCCCCyo..",
    "..okCCCCCCCo..",
    "..okcCCCCCCo..",
    "..okcCCCCCCo..",
    "..okcCCCCCCco.",
    "..okkccccccko.",
    "...ooooooooo..",
]

ARM = [
    "oCCo",
    "oCco",
    "oCco",
    "oCco",
    "oCco",
    "oCko",
    "okko",
    "oSso",
    "osso",
    ".oo.",
]

ARM_SIDE = [
    ".oo.",
    "oCCo",
    "oCco",
    "oCco",
    "oCco",
    "oCko",
    "okko",
    "oSso",
    "osso",
    ".oo.",
]


def _leg(c, x, top, bottom, shade=False):
    """Trouser leg + shoe, 5px wide incl. outline. bottom = y of the outline row under the shoe."""
    fill, dark = ("p", "p") if shade else ("P", "p")
    pal = PAL
    for y in range(top, bottom - 2):
        c.set(x, y, pal["o"])
        c.set(x + 1, y, pal[fill])
        c.set(x + 2, y, pal[fill])
        c.set(x + 3, y, pal[dark])
        c.set(x + 4, y, pal["o"])
    # shoe (2 rows) + outline row
    for i, y in enumerate((bottom - 2, bottom - 1)):
        c.set(x, y, pal["o"])
        c.set(x + 1, y, pal["b"] if i == 0 and not shade else pal["B"])
        c.set(x + 2, y, pal["B"])
        c.set(x + 3, y, pal["B"])
        c.set(x + 4, y, pal["o"])
    c.hline(x, bottom, 5, pal["o"])


def _side_leg(c, x, top, bottom, shade=False):
    """Side-view leg: shoe points right (one pixel longer)."""
    _leg(c, x, top, bottom, shade)
    c.set(x + 4, bottom - 2, PAL["B"])
    c.set(x + 4, bottom - 1, PAL["B"])
    c.set(x + 5, bottom - 2, PAL["o"])
    c.set(x + 5, bottom - 1, PAL["o"])
    c.set(x + 4, bottom, PAL["o"])


FEET_Y = FRAME_H - 1 - PIVOT[1]   # y of the outline row under the shoes (45)
LEG_TOP = FEET_Y - 6              # first trouser row (just under the coat hem)
TORSO_Y = LEG_TOP - 12 + 1        # torso is 12 rows; hem overlaps leg top by one row
HEAD_Y = TORSO_Y - 15             # chin outline overlaps collar


def _front_back(head, torso, bob, lift_left, lift_right, arm_left, arm_right):
    c = Canvas(FRAME_W, FRAME_H)
    # legs (drawn first so the coat hem overlaps them)
    _leg(c, 11, LEG_TOP - bob, FEET_Y - lift_left)
    _leg(c, 16, LEG_TOP - bob, FEET_Y - lift_right)
    ty, hy = TORSO_Y - bob, HEAD_Y - bob
    c.ascii(ARM, PAL, 7, ty + 2 + arm_left)
    c.ascii(ARM, PAL, 21, ty + 2 + arm_right)
    c.ascii(torso, PAL, 9, ty)
    c.ascii(head, PAL, 8, hy)
    return c


def _side(bob, near_dx, far_dx, near_lift, far_lift, arm_dx):
    c = Canvas(FRAME_W, FRAME_H)
    ty, hy = TORSO_Y - bob, HEAD_Y - bob
    # far arm (behind body), far leg, near leg, torso, near arm, head
    c.ascii(ARM_SIDE, {**PAL, "C": PAL["c"], "c": PAL["k"], "S": PAL["s"]}, 14 - arm_dx, ty + 2)
    _side_leg(c, 13 + far_dx, LEG_TOP - bob, FEET_Y - far_lift, shade=True)
    _side_leg(c, 13 + near_dx, LEG_TOP - bob, FEET_Y - near_lift)
    c.ascii(TORSO_RIGHT, PAL, 8, ty)
    c.ascii(ARM_SIDE, PAL, 14 + arm_dx, ty + 2)
    c.ascii(HEAD_RIGHT, PAL, 8, hy)
    return c


def build_frames():
    """Returns dict direction -> list of 6 frames: idle0, idle1, walk0..walk3."""
    frames = {}
    for name, head, torso in (("down", HEAD_DOWN, TORSO_DOWN), ("up", HEAD_UP, TORSO_UP)):
        frames[name] = [
            _front_back(head, torso, 0, 0, 0, 0, 0),
            _front_back(head, torso, 1, 0, 0, 0, 0),
            # walk: contact / passing / contact / passing
            _front_back(head, torso, 0, 0, 1, -1, 1),
            _front_back(head, torso, 1, 0, 0, 0, 0),
            _front_back(head, torso, 0, 1, 0, 1, -1),
            _front_back(head, torso, 1, 0, 0, 0, 0),
        ]
    right = [
        _side(0, 0, 0, 0, 0, 0),
        _side(1, 0, 0, 0, 0, 0),
        _side(0, 3, -3, 0, 0, -2),
        _side(1, 1, -1, 0, 1, 0),
        _side(0, -3, 3, 0, 0, 2),
        _side(1, -1, 1, 1, 0, 0),
    ]
    frames["right"] = right
    frames["left"] = [f.flipped() for f in right]
    return frames


ROW_ORDER = ("down", "left", "right", "up")


def build_sheet():
    from pixel import sheet
    frames = build_frames()
    ordered = [f for d in ROW_ORDER for f in frames[d]]
    return sheet(ordered, 6)
