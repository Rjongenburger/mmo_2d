"""Medieval hero (villager in a linen tunic): 32x48 frames.

Rows (directions): down, left, right, up.
Columns: idle0, idle1, walk0..walk3, cast0 (wind-up), cast1 (release), fly0..fly1 (riding the gale feather).
"""
from pixel import Canvas, rgb, sheet, with_alpha

FRAME_W, FRAME_H = 32, 48
PIVOT = (16, 2)
ROW_ORDER = ("down", "left", "right", "up")
COLUMNS = ("idle0", "idle1", "walk0", "walk1", "walk2", "walk3", "cast0", "cast1", "fly0", "fly1")

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
FLAME = (rgb("#fff8d6"), rgb("#ffd25a"), rgb("#ff8a2a"), rgb("#d9431c"))  # core, inner, mid, edge
FEATHER = (rgb("#ffffff"), rgb("#e4f4ff"), rgb("#9fc6e0"), rgb("#8ff0ff"))  # highlight, vane, shade, glow
QUILL = rgb("#d6b98a")

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


def _flame(c, cx, cy, big=False):
    """Small fire orb in the hero's hand (big = the moment the bolt is released)."""
    core, inner, mid, edge = FLAME
    r = 3 if big else 2
    glow = Canvas(FRAME_W, FRAME_H)
    glow.soft_ellipse(cx + 0.5, cy + 0.5, r + 2.5, r + 2.5, with_alpha(inner, 110), falloff=0.8)
    c.blit(glow, 0, 0)
    for y in range(cy - r, cy + r + 1):
        for x in range(cx - r, cx + r + 1):
            d = abs(x - cx) + abs(y - cy)
            if d > r + (1 if big else 0):
                continue
            col = core if d == 0 else inner if d <= r - 1 else mid if d == r else edge
            c.set(x, y, col)
    for dx in ((-1, 1) if big else (0,)):  # flickering tongues
        c.set(cx + dx, cy - r - 1, mid)
    c.set(cx, cy - r - (2 if big else 1), inner)


def _feather(c, x0, y0, length=13, droop=0):
    """The gale feather held overhead, drawn left to right with a glowing edge."""
    hi, vane, shade, glow = FEATHER
    for i in range(length):
        x, y = x0 + i, y0 + (droop if i > length * 0.6 else 0)
        width = 2 if 2 < i < length - 2 else 1
        for k in range(1, width + 1):
            c.set(x, y - k, vane if k < width else hi)
            c.set(x, y + k, shade if k == width else vane)
        c.set(x, y, hi if i % 3 else shade)
    c.set(x0 - 1, y0, QUILL)
    c.set(x0 - 2, y0 + 1, QUILL)
    for (x, y) in ((x0 + 2, y0 - 3), (x0 + length - 1, y0 - 2), (x0 + length // 2, y0 + 3)):
        c.set(x, y, with_alpha(glow, 200))



def _cast_down(release):
    c = _front_back(HEAD_DOWN, TORSO_DOWN, arms=False)
    hand = (16, TORSO_Y + 9) if release else (16, TORSO_Y + 5)
    _limb(c, (11, TORSO_Y + 3), (hand[0] - 1, hand[1]))
    _limb(c, (20, TORSO_Y + 3), (hand[0], hand[1]))
    _flame(c, hand[0], hand[1] + (3 if release else 1), big=release)
    return c


def _cast_up(release):
    c = _front_back(HEAD_UP, TORSO_UP, arms=False)
    _limb(c, (11, TORSO_Y + 3), (10, TORSO_Y + 8))
    if release:
        _flame(c, 16, HEAD_Y - 4, big=True)
        _limb(c, (20, TORSO_Y + 3), (21, TORSO_Y + 7))
    else:
        _limb(c, (20, TORSO_Y + 3), (23, TORSO_Y - 2))
        _flame(c, 24, TORSO_Y - 4)
    return c


def _cast_right(release):
    c = _side(arm=False)
    if release:
        _limb(c, (16, TORSO_Y + 4), (24, TORSO_Y + 4))
        _flame(c, 27, TORSO_Y + 3, big=True)
    else:
        _limb(c, (15, TORSO_Y + 4), (11, TORSO_Y + 5))
        _flame(c, 9, TORSO_Y + 3)
    return c


def _fly_front(head, torso, flutter, feather_in_front):
    c = Canvas(FRAME_W, FRAME_H)
    # legs dangle, one slightly higher than the other
    _leg(c, 11, LEG_TOP, FEET_Y - (1 + flutter))
    _leg(c, 16, LEG_TOP, FEET_Y - (2 - flutter))
    if not feather_in_front:
        _feather(c, 9, 9 - flutter, droop=flutter)
    _limb(c, (11, TORSO_Y + 3), (13, 11))
    _limb(c, (20, TORSO_Y + 3), (19, 11))
    c.ascii(torso, PAL, 9, TORSO_Y)
    c.ascii(head, PAL, 8, HEAD_Y)
    if feather_in_front:
        _feather(c, 9, 9 - flutter, droop=flutter)
    return c


def _fly_right(flutter):
    c = Canvas(FRAME_W, FRAME_H)
    _leg(c, 9 - flutter, LEG_TOP, FEET_Y - 2, shade=True, side=True)
    _leg(c, 11 + flutter, LEG_TOP, FEET_Y - 1, side=True)
    c.ascii(TORSO_RIGHT, PAL, 8, TORSO_Y)
    _limb(c, (15, TORSO_Y + 3), (18, 11))
    c.ascii(HEAD_RIGHT, PAL, 8, HEAD_Y)
    _feather(c, 12, 9 - flutter, droop=flutter)
    return c


def build_frames():
    frames = {}
    for name, head, torso, cast in (("down", HEAD_DOWN, TORSO_DOWN, _cast_down), ("up", HEAD_UP, TORSO_UP, _cast_up)):
        frames[name] = [
            _front_back(head, torso),
            _front_back(head, torso, bob=1),
            _front_back(head, torso, 0, 0, 1, -1, 1),
            _front_back(head, torso, 1),
            _front_back(head, torso, 0, 1, 0, 1, -1),
            _front_back(head, torso, 1),
            cast(False),
            cast(True),
            _fly_front(head, torso, 0, name == "down"),
            _fly_front(head, torso, 1, name == "down"),
        ]
    right = [
        _side(),
        _side(bob=1),
        _side(0, 3, -3, 0, 0, -2),
        _side(1, 1, -1, 0, 1, 0),
        _side(0, -3, 3, 0, 0, 2),
        _side(1, -1, 1, 1, 0, 0),
        _cast_right(False),
        _cast_right(True),
        _fly_right(0),
        _fly_right(1),
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
    """48x48 bust (head and shoulders, 2x) for dialogue portraits."""
    c = Canvas(24, 24)
    c.ascii(TORSO_DOWN, PAL, 5, 14)
    c.ascii(HEAD_DOWN, PAL, 4, 1)
    return c.scaled(2)
