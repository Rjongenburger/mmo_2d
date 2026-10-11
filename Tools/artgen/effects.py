"""Small animated effects shared by the scenes."""
import math

from pixel import Canvas, rgb, with_alpha

OUT = rgb("#2b2433")
BAYER4 = [[0, 8, 2, 10], [12, 4, 14, 6], [3, 11, 1, 9], [15, 7, 13, 5]]
PORTAL_COLS = [rgb("#24123f"), rgb("#43207a"), rgb("#6a3dbf"), rgb("#9a74f2"), rgb("#c9b6ff"), rgb("#f2ecff"), rgb("#ffffff")]
PORTAL_FRAMES = 8


def portal(frame):
    """Upright swirling oval, 64x64."""
    size = 64
    c = Canvas(size, size)
    cx, cy, rx, ry = 31.5, 31.5, 26.0, 30.5
    phase = frame / PORTAL_FRAMES * math.tau
    for y in range(size):
        for x in range(size):
            dx, dy = (x + 0.5 - cx) / rx, (y + 0.5 - cy) / ry
            r = math.hypot(dx, dy)
            if r > 1.0:
                continue
            a = math.atan2(dy, dx)
            swirl = math.sin(3 * a + 10 * r - phase)
            v = (1 - r) * 0.85 + swirl * 0.22 + (0.35 if r > 0.9 else 0)
            v += (BAYER4[y % 4][x % 4] / 16 - 0.5) * 0.12
            idx = max(0, min(len(PORTAL_COLS) - 1, int(v * (len(PORTAL_COLS) - 0.5) + 0.6)))
            if r < 0.16:
                idx = len(PORTAL_COLS) - 1
            alpha = 255 if r < 0.93 else 200
            c.replace(x, y, with_alpha(PORTAL_COLS[idx], alpha))
    for i in range(6):  # orbiting sparks on the rim
        a = phase * 1.5 + i * math.tau / 6
        x, y = round(cx + math.cos(a) * (rx + 1.5)), round(cy + math.sin(a) * (ry + 1.5))
        c.set(x, y, PORTAL_COLS[5])
    return c


def sparkle(frame):
    c = Canvas(7, 7)
    col, core = rgb("#fff4b0"), rgb("#ffffff")
    arm = (1, 2, 3, 1)[frame]
    for i in range(1, arm + 1):
        for dx, dy in ((i, 0), (-i, 0), (0, i), (0, -i)):
            c.set(3 + dx, 3 + dy, with_alpha(col, 255 if i < arm else 170))
    if frame == 2:
        for dx, dy in ((1, 1), (-1, -1), (1, -1), (-1, 1)):
            c.set(3 + dx, 3 + dy, with_alpha(col, 160))
    c.set(3, 3, core)
    return c


def dust(frame):
    c = Canvas(16, 12)
    col = rgb("#d9cfc0")
    r = 2 + frame * 1.6
    a = (220, 170, 110, 50)[frame]
    for bx, by in ((5, 8), (10, 8), (8, 6)):
        c.ellipse(bx, by - frame * 0.6, r * 0.8, r * 0.55, with_alpha(col, a))
    return c


def wood_chip():
    c = Canvas(3, 2)
    c.hline(0, 0, 3, rgb("#c99a62"))
    c.set(1, 1, rgb("#8a5f38"))
    return c


def wind_streak():
    c = Canvas(24, 1)
    for x in range(24):
        t = x / 23
        c.set(x, 0, with_alpha(rgb("#ffffff"), int(200 * math.sin(t * math.pi) ** 1.5)))
    return c


def snowflake(big):
    c = Canvas(2, 2)
    if big:
        c.rect(0, 0, 2, 2, rgb("#ffffff"))
        c.set(1, 1, rgb("#d8e2f2"))
    else:
        c.set(0, 0, rgb("#ffffff"))
    return c


def ember():
    c = Canvas(2, 2)
    c.set(0, 0, rgb("#ffd25a"))
    c.set(1, 0, rgb("#ff9a2e"))
    c.set(0, 1, rgb("#ff9a2e"))
    return c


def zzz():
    c = Canvas(7, 7)
    col = rgb("#cfe3ff")
    c.hline(1, 1, 5, col)
    for i in range(4):
        c.set(4 - i, 2 + i, col)
    c.hline(1, 5, 5, col)
    c.outline(rgb("#2b3a66"))
    return c


def exclaim():
    c = Canvas(11, 15)
    c.rect(1, 1, 9, 10, rgb("#ffffff"))
    c.set(1, 1, (0, 0, 0, 0))
    c.set(9, 1, (0, 0, 0, 0))
    c.set(1, 10, (0, 0, 0, 0))
    c.set(9, 10, (0, 0, 0, 0))
    c.rect(3, 11, 3, 2, rgb("#ffffff"))
    c.set(3, 13, rgb("#ffffff"))
    red = rgb("#d93b3b")
    c.rect(5, 2, 2, 5, red)
    c.rect(5, 8, 2, 2, red)
    c.outline(OUT)
    return c


def hint_arrow():
    c = Canvas(13, 13)
    yel, yel_hi, yel_dark = rgb("#ffd84a"), rgb("#fff3a6"), rgb("#d19b14")
    c.rect(4, 1, 5, 6, yel)
    for i in range(6):
        c.hline(1 + i, 6 + i, 11 - 2 * i, yel)
    c.vline(4, 1, 6, yel_hi)
    for i in range(5):
        c.set(1 + i, 6 + i, yel_hi)
    for i in range(6):
        c.set(11 - i - 1, 6 + i, yel_dark)
    c.outline(OUT)
    return c


def portal_spark():
    c = Canvas(3, 3)
    c.set(1, 1, rgb("#ffffff"))
    for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
        c.set(1 + dx, 1 + dy, with_alpha(rgb("#c9b6ff"), 200))
    return c


def smoke():
    c = Canvas(8, 8)
    c.ellipse(4, 4, 3.5, 3.5, with_alpha(rgb("#8a817c"), 110))
    c.ellipse(3.5, 3.5, 2, 2, with_alpha(rgb("#aaa19c"), 90))
    return c


def glow_disc(size, color, alpha=120):
    """Soft additive-looking disc, used behind magic items."""
    c = Canvas(size, size)
    c.soft_ellipse(size / 2, size / 2, size / 2 - 0.5, size / 2 - 0.5, with_alpha(color, alpha), falloff=0.9)
    return c


# ---------------------------------------------------------------- combat
FIRE = [rgb("#fff8d6"), rgb("#ffe27a"), rgb("#ffb03a"), rgb("#f2671f"), rgb("#b8321a")]


def fireball(frame):
    """16x12 bolt pointing right (rotated in-engine to its direction of travel)."""
    c = Canvas(16, 12)
    cx, cy = 11, 6
    flick = (0, 1, -1)[frame]
    # trailing tongues
    for i, (length, dy) in enumerate(((7 + flick, 0), (5 - flick, -2), (5 + flick, 2))):
        for k in range(length):
            x = cx - 2 - k
            fade = k / max(1, length)
            col = FIRE[min(4, 2 + int(fade * 3))]
            c.set(x, cy + dy + (1 if (k + frame) % 3 == 0 and dy else 0), with_alpha(col, int(255 * (1 - fade * 0.6))))
    c.ellipse(cx + 0.5, cy + 0.5, 3.6, 3.4, FIRE[3])
    c.ellipse(cx + 0.8, cy + 0.5, 2.6, 2.4, FIRE[2])
    c.ellipse(cx + 1.0, cy + 0.3, 1.6, 1.5, FIRE[1])
    c.set(cx + 1, cy, FIRE[0])
    c.set(cx + 2, cy, FIRE[0])
    return c


def explosion(frame):
    """32x32 burst: flash, expanding fire ring, smoke."""
    c = Canvas(32, 32)
    r = 4 + frame * 2.6
    if frame <= 3:
        c.ellipse(16, 16, r, r, with_alpha(FIRE[3], 230 - frame * 40))
        c.ellipse(16, 16, r * 0.75, r * 0.75, with_alpha(FIRE[2], 240 - frame * 50))
        c.ellipse(16, 16, max(1, r * 0.45 - frame), max(1, r * 0.45 - frame), with_alpha(FIRE[0], 255 - frame * 60))
    else:
        smoke = rgb("#6e6470")
        for (dx, dy, rr) in ((-4, -2, 4), (3, -4, 3.5), (2, 3, 4), (-3, 4, 3)):
            c.ellipse(16 + dx, 16 + dy - (frame - 4) * 2, rr + frame - 4, rr + frame - 4, with_alpha(smoke, 150 - (frame - 4) * 60))
    for i in range(8):  # sparks
        a = i / 8 * math.tau + frame * 0.3
        d = r + 2
        x, y = round(16 + math.cos(a) * d), round(16 + math.sin(a) * d)
        if frame < 5:
            c.set(x, y, FIRE[1])
    return c


def feather_puff():
    c = Canvas(5, 3)
    c.hline(0, 1, 4, rgb("#3a3150"))
    c.set(4, 0, rgb("#5b4f7a"))
    c.set(1, 0, rgb("#5b4f7a"))
    c.set(2, 2, rgb("#28213a"))
    return c


def heart(state):
    """9x9 heart: full, half or empty."""
    c = Canvas(9, 9)
    shape = [".##.##...", "#######..", "#######..", "#######..", ".#####...", "..###....", "...#....."]
    red, red_hi, red_sh, empty = rgb("#e8413c"), rgb("#ff9b8a"), rgb("#a8242a"), rgb("#3b2a36")
    for y, row in enumerate(shape):
        for x, ch in enumerate(row):
            if ch != "#":
                continue
            filled = state == "full" or (state == "half" and x <= 2)
            col = (red_hi if (x, y) in ((1, 1), (2, 1), (1, 2)) else red_sh if y >= 4 or x >= 6 else red) if filled else empty
            c.set(x + 1, y + 1, col)
    c.outline(rgb("#160f14"))
    return c


def continue_arrow():
    c = Canvas(9, 6)
    gold, gold_hi = rgb("#e9c46a"), rgb("#fff1b8")
    for i in range(4):
        c.hline(1 + i, 1 + i, 7 - 2 * i, gold)
    c.hline(1, 1, 7, gold_hi)
    c.outline(rgb("#0b0d16"))
    return c


def white_pixel():
    c = Canvas(4, 4)
    c.rect(0, 0, 4, 4, rgb("#ffffff"))
    return c
