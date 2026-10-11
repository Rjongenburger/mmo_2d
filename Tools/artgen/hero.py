"""Medieval hero (villager in a linen tunic): 32x48 frames.

Rows (directions): down, left, right, up.
Columns: idle0, idle1, walk0..walk3, chop0 (wind-up), chop1 (strike).
"""
from pixel import Canvas, rgb, sheet

FRAME_W, FRAME_H = 32, 48
PIVOT = (16, 2)
ROW_ORDER = ("down", "left", "right", "up")
COLUMNS = ("idle0", "idle1", "walk0", "walk1", "walk2", "walk3", "chop0", "chop1")

PAL = {
    "o": rgb("#2b2433"),
    "H": rgb("#6b4329"), "h": rgb("#93603a"), "d": rgb("#452a1a"),
    "S": rgb("#f3cba5"), "s": rgb("#d9a17e"), "r": rgb("#eaa08e"),
    "e": rgb("#2b2433"), "w": rgb("#ffffff"), "m": rgb("#a8574a"), "z": rgb("#b07a62"),
    "C": rgb("#ece0c4"), "c": rgb("#cdbb93"), "k": rgb("#a7926a"),
    "L": rgb("#8a6a42"),
    "B": rgb("#7a5434"), "b": rgb("#d4ad55"),
    "P": rgb("#6e5640"), "p": rgb("#4f3d2d"),
    "F": rgb("#4a3628"), "f": rgb("#6e5340"),
}
AXE_HANDLE, AXE_HANDLE_HI = rgb("#7a5232"), rgb("#a3714a")
AXE_HEAD = (rgb("#f0c08a"), rgb("#c8803f"), rgb("#8a4f22"))  # bronze: highlight, base, shade

HEAD_DOWN = [
    ".....o.oo.o.....",
    "....oHoHHoHo....",
    "..ooHHHhhHHHoo..",
    ".oHHHhhHHHHHHHo.",
    "oHHHhHHHHHHHHHdo",
    "oHHHHHHHHHHHHHdo",
    "oHHdHHHHHdHHHHdo",
    "oHdSdHHHdSdHHddo",
    "odSSSdHdSSSSdHdo",
    "odSSSSSSSSSSSSdo",
    "odSSeeSSSSeeSSdo",
    "odSSewSSSSewSSdo",
    ".oSrSSSSSSSSrSo.",
    ".osSSSSSmmSSSso.",
    "..oosSSSSSSsoo..",
    "....oooooooo....",
]

# Sleeping / startled variants used by the bed props in the house.
HEAD_DOWN_ASLEEP = HEAD_DOWN[:10] + ["odSSzzSSSSzzSSdo", "odSSSSSSSSSSSSdo"] + HEAD_DOWN[12:]
HEAD_DOWN_STARTLED = HEAD_DOWN[:9] + ["odSSeeSSSSeeSSdo", "odSewwSSSSewwSdo", "odSSeeSSSSeeSSdo",
                                      ".oSrSSSSSSSSrSo.", ".osSSSSSooSSSso.", "..oosSSSSSSsoo..", "....oooooooo...."]

HEAD_UP = [
    ".....o.oo.o.....",
    "....oHoHHoHo....",
    "..ooHHHHHHHHoo..",
    ".oHHHHHhhHHHHHo.",
    "oHHHHHhHHHHHHHHo",
    "oHHHHHHHHHHHHHdo",
    "oHHHHHHHHHHHHHdo",
    "oHHHHHHHdHHHHHdo",
    "odHHHHHHdHHHHHdo",
    "odHHdHHHHHHHdHdo",
    "odHHHHHHHHHHHddo",
    "oddHHHdHHdHHHddo",
    ".oddHHHHHHHHddo.",
    ".osddddddddddso.",
    "..oosSSSSSSsoo..",
    "....oooooooo....",
]

HEAD_RIGHT = [
    "....o.oo.o......",
    "...oHoHHoHo.....",
    "..ooHHHhhHHoo...",
    ".oHHHHhhhHHHHo..",
    "oHHHHHHhHHHHHHo.",
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
    "..ooCCSSCCoo..",
    ".oCCCCLSCCCCo.",
    ".oCCCCCLCCCCo.",
    ".oCCCCCCCCCCo.",
    ".oCCCCCCCCCco.",
    ".oBBBBBbBBBBo.",
    ".ocCCCCCCCCco.",
    ".ocCCCCCCCCco.",
    ".ocCCCCCCCCco.",
    ".okccccccccko.",
    "..oooooooooo..",
]

TORSO_UP = [
    "....osSSso....",
    "..ooCCCCCCoo..",
    ".oCCCCCCCCCCo.",
    ".oCCCCCCCCCCo.",
    ".oCcCCCCCCcCo.",
    ".oCcCCCCCCcCo.",
    ".oBBBBBBBBBBo.",
    ".ocCCCCCCCCco.",
    ".ocCCCCCCCCco.",
    ".ocCCCCcCCCco.",
    ".okccccccccko.",
    "..oooooooooo..",
]

TORSO_RIGHT = [
    "....osSso.....",
    "...ocCCSCo....",
    "..oCCCCCLSo...",
    "..oCCCCCCLo...",
    "..okCCCCCCo...",
    "..okCCCCCCCo..",
    "..oBBBBBBbBo..",
    "..okcCCCCCCo..",
    "..okcCCCCCCo..",
    "..okcCCCCCCco.",
    "..okkccccccko.",
    "...ooooooooo..",
]

ARM = ["oCCo", "oCco", "oCco", "oCco", "oCco", "oCko", "okko", "oSso", "osso", ".oo."]
ARM_SIDE = [".oo.", "oCCo", "oCco", "oCco", "oCco", "oCko", "okko", "oSso", "osso", ".oo."]

FEET_Y = FRAME_H - 1 - PIVOT[1]
LEG_TOP = FEET_Y - 6
TORSO_Y = LEG_TOP - 12 + 1
HEAD_Y = TORSO_Y - 15


def _leg(c, x, top, bottom, shade=False, side=False):
    pal = PAL
    fill, dark = ("p", "p") if shade else ("P", "p")
    for y in range(top, bottom - 2):
        for i, k in enumerate(("o", fill, fill, dark, "o")):
            c.set(x + i, y, pal[k])
    for i, y in enumerate((bottom - 2, bottom - 1)):
        for j, k in enumerate(("o", "f" if i == 0 and not shade else "F", "F", "F", "o")):
            c.set(x + j, y, pal[k])
    c.hline(x, bottom, 5, pal["o"])
    if side:  # toe points forward
        c.set(x + 4, bottom - 2, pal["F"])
        c.set(x + 4, bottom - 1, pal["F"])
        c.set(x + 5, bottom - 2, pal["o"])
        c.set(x + 5, bottom - 1, pal["o"])
        c.set(x + 4, bottom, pal["o"])


def _limb(c, p0, p1, sleeve="C", shade="c"):
    """Draws an outlined 3px sleeve from shoulder p0 to hand p1 (hand is skin)."""
    layer = Canvas(FRAME_W, FRAME_H)
    (x0, y0), (x1, y1) = p0, p1
    steps = max(abs(x1 - x0), abs(y1 - y0), 1)
    for i in range(steps + 1):
        t = i / steps
        x, y = round(x0 + (x1 - x0) * t), round(y0 + (y1 - y0) * t)
        col = PAL["S"] if t > 0.82 else PAL[sleeve]
        for dx in (0, 1):
            for dy in (0, 1):
                layer.set(x + dx, y + dy, col)
        layer.set(x + 1, y + 1, PAL["s"] if t > 0.82 else PAL[shade])
    layer.outline(PAL["o"])
    c.blit(layer, 0, 0)


AXE_BLADE_WIDTHS = (2, 4, 5, 5, 4, 2)  # wedge profile, measured outward from the handle


def _axe(c, hand, tip, blade=None):
    """2px handle from hand to tip with a wedge-shaped bronze head at the tip.

    blade: (dx, dy) unit direction the cutting edge faces; defaults to sideways for
    vertical handles and downwards for horizontal ones.
    """
    layer = Canvas(FRAME_W, FRAME_H)
    (x0, y0), (x1, y1) = hand, tip
    dx, dy = x1 - x0, y1 - y0
    vertical = abs(dy) >= abs(dx)
    layer.line(x0, y0, x1, y1, AXE_HANDLE)
    if vertical:
        layer.line(x0 + 1, y0, x1 + 1, y1, AXE_HANDLE_HI)
    else:
        layer.line(x0, y0 - 1, x1, y1 - 1, AXE_HANDLE_HI)
    if blade is None:
        blade = ((1 if dx >= 0 else -1), 0) if vertical else (0, (1 if dy >= 0 else -1))
    hi, base, shade = AXE_HEAD
    n = len(AXE_BLADE_WIDTHS)
    for i, width in enumerate(AXE_BLADE_WIDTHS):
        for j in range(width):
            col = hi if j == width - 1 else (shade if i >= n - 2 else base)
            if blade[0]:  # edge faces left/right; rows run along the handle
                layer.set(x1 + blade[0] * j, y1 - 2 + i, col)
            else:          # edge faces up/down
                layer.set(x1 - 2 + i, y1 + blade[1] * j, col)
    layer.outline(PAL["o"])
    c.blit(layer, 0, 0)


def _front_back(head, torso, bob=0, lift_l=0, lift_r=0, arm_l=0, arm_r=0, arms=True):
    c = Canvas(FRAME_W, FRAME_H)
    _leg(c, 11, LEG_TOP - bob, FEET_Y - lift_l)
    _leg(c, 16, LEG_TOP - bob, FEET_Y - lift_r)
    ty, hy = TORSO_Y - bob, HEAD_Y - bob
    if arms:
        c.ascii(ARM, PAL, 7, ty + 2 + arm_l)
        c.ascii(ARM, PAL, 21, ty + 2 + arm_r)
    c.ascii(torso, PAL, 9, ty)
    c.ascii(head, PAL, 8, hy)
    return c


def _side(bob=0, near_dx=0, far_dx=0, near_lift=0, far_lift=0, arm_dx=0, arm=True):
    c = Canvas(FRAME_W, FRAME_H)
    ty, hy = TORSO_Y - bob, HEAD_Y - bob
    if arm:
        c.ascii(ARM_SIDE, {**PAL, "C": PAL["c"], "c": PAL["k"], "S": PAL["s"]}, 14 - arm_dx, ty + 2)
    _leg(c, 13 + far_dx, LEG_TOP - bob, FEET_Y - far_lift, shade=True, side=True)
    _leg(c, 13 + near_dx, LEG_TOP - bob, FEET_Y - near_lift, side=True)
    c.ascii(TORSO_RIGHT, PAL, 8, ty)
    if arm:
        c.ascii(ARM_SIDE, PAL, 14 + arm_dx, ty + 2)
    c.ascii(HEAD_RIGHT, PAL, 8, hy)
    return c


def _chop_down(strike):
    c = Canvas(FRAME_W, FRAME_H)
    _leg(c, 11, LEG_TOP, FEET_Y)
    _leg(c, 16, LEG_TOP, FEET_Y)
    if not strike:
        _axe(c, (16, 11), (17, 3))
        _limb(c, (11, TORSO_Y + 3), (14, 9))
        _limb(c, (20, TORSO_Y + 3), (17, 9))
        c.ascii(TORSO_DOWN, PAL, 9, TORSO_Y)
        c.ascii(HEAD_DOWN, PAL, 8, HEAD_Y)
    else:
        c.ascii(TORSO_DOWN, PAL, 9, TORSO_Y + 1)
        c.ascii(HEAD_DOWN, PAL, 8, HEAD_Y + 1)
        _axe(c, (16, TORSO_Y + 9), (16, TORSO_Y + 15), blade=(0, 1))
        _limb(c, (11, TORSO_Y + 4), (15, TORSO_Y + 9))
        _limb(c, (20, TORSO_Y + 4), (16, TORSO_Y + 9))
    return c


def _chop_up(strike):
    c = Canvas(FRAME_W, FRAME_H)
    _leg(c, 11, LEG_TOP, FEET_Y)
    _leg(c, 16, LEG_TOP, FEET_Y)
    if not strike:
        c.ascii(TORSO_UP, PAL, 9, TORSO_Y)
        _axe(c, (16, 11), (15, 3), blade=(-1, 0))
        _limb(c, (11, TORSO_Y + 3), (14, 9))
        _limb(c, (20, TORSO_Y + 3), (17, 9))
        c.ascii(HEAD_UP, PAL, 8, HEAD_Y)
    else:
        _limb(c, (11, TORSO_Y + 4), (14, TORSO_Y + 10))
        _limb(c, (20, TORSO_Y + 4), (17, TORSO_Y + 10))
        c.ascii(TORSO_UP, PAL, 9, TORSO_Y + 1)
        c.ascii(HEAD_UP, PAL, 8, HEAD_Y + 1)
    return c


def _chop_right(strike):
    c = Canvas(FRAME_W, FRAME_H)
    _leg(c, 11, LEG_TOP, FEET_Y, shade=True, side=True)
    _leg(c, 15, LEG_TOP, FEET_Y, side=True)
    if not strike:
        c.ascii(TORSO_RIGHT, PAL, 8, TORSO_Y)
        _axe(c, (11, 11), (7, 4), blade=(-1, 0))
        _limb(c, (14, TORSO_Y + 3), (11, 10))
        c.ascii(HEAD_RIGHT, PAL, 8, HEAD_Y)
    else:
        c.ascii(TORSO_RIGHT, PAL, 9, TORSO_Y + 1)
        c.ascii(HEAD_RIGHT, PAL, 9, HEAD_Y + 1)
        _axe(c, (23, TORSO_Y + 5), (27, TORSO_Y + 9), blade=(1, 0))
        _limb(c, (16, TORSO_Y + 4), (23, TORSO_Y + 5))
    return c


def build_frames():
    frames = {}
    for name, head, torso, chop in (("down", HEAD_DOWN, TORSO_DOWN, _chop_down), ("up", HEAD_UP, TORSO_UP, _chop_up)):
        frames[name] = [
            _front_back(head, torso),
            _front_back(head, torso, bob=1),
            _front_back(head, torso, 0, 0, 1, -1, 1),
            _front_back(head, torso, 1),
            _front_back(head, torso, 0, 1, 0, 1, -1),
            _front_back(head, torso, 1),
            chop(False),
            chop(True),
        ]
    right = [
        _side(),
        _side(bob=1),
        _side(0, 3, -3, 0, 0, -2),
        _side(1, 1, -1, 0, 1, 0),
        _side(0, -3, 3, 0, 0, 2),
        _side(1, -1, 1, 1, 0, 0),
        _chop_right(False),
        _chop_right(True),
    ]
    frames["right"] = right
    frames["left"] = [f.flipped() for f in right]
    return frames


def build_sheet():
    frames = build_frames()
    return sheet([f for d in ROW_ORDER for f in frames[d]], len(COLUMNS))


def frame_names(prefix="Hero"):
    return [f"{prefix}_{d}_{col}" for d in ROW_ORDER for col in COLUMNS]


def head_portrait():
    """2x upscaled head for dialogue portraits."""
    c = Canvas(18, 18)
    c.ascii(HEAD_DOWN, PAL, 1, 1)
    return c.scaled(2)
