"""Cosy cottage bedroom for the intro cutscene (night)."""
import math
import random

import hero
from pixel import Canvas, rgb, with_alpha, mix

T = 32
OUT = rgb("#2b2433")
FL_HI, FL, FL_SH, FL_GAP = rgb("#b07a4f"), rgb("#94633f"), rgb("#7a4f31"), rgb("#4e301d")
PL_HI, PL, PL_SH = rgb("#efe2c6"), rgb("#dccba7"), rgb("#c2ad86")
BM_HI, BM, BM_SH = rgb("#7a5236"), rgb("#5c3c26"), rgb("#432a1a")
ST_HI, ST, ST_SH = rgb("#a39a8e"), rgb("#857c71"), rgb("#655d55")
CAP, CAP_RIM, CAP_EDGE = rgb("#33261f"), rgb("#4d3a2e"), rgb("#1c1512")
QUILT = [rgb("#a8413a"), rgb("#c98a3a"), rgb("#5a7a4a"), rgb("#3e5a8a")]
LINEN, LINEN_SH = rgb("#f2ead8"), rgb("#cfc2a6")


def floor_plank(variant):
    rnd = random.Random(10 + variant)
    c = Canvas(T, T)
    for row in range(4):
        y0 = row * 8
        base = (FL, mix(FL, FL_HI, 0.25), mix(FL, FL_SH, 0.3))[(row + variant) % 3]
        c.rect(0, y0, T, 8, base)
        c.hline(0, y0, T, mix(base, FL_HI, 0.5))
        c.hline(0, y0 + 7, T, FL_GAP)
        seam = (rnd.randrange(4, 28) + row * 11) % T
        c.vline(seam, y0, 7, FL_GAP)
        c.vline(seam + 1, y0, 7, mix(base, FL_HI, 0.4))
        for _ in range(2):  # grain
            gx, gy = rnd.randrange(T - 6), y0 + rnd.randrange(2, 6)
            c.hline(gx, gy, rnd.randrange(3, 7), FL_SH)
        c.set((seam + 3) % T, y0 + 3, FL_GAP)
    return c


def wall_face(kind):
    c = Canvas(T, 2 * T)
    c.hline(0, 0, T, BM_SH)
    c.hline(0, 1, T, BM)
    c.rect(0, 2, T, 36, PL)
    for y in range(2, 38):  # subtle plaster texture
        for x in range(T):
            if (x * 7 + y * 13) % 29 == 0:
                c.set(x, y, PL_SH)
            elif (x * 5 + y * 11) % 37 == 0:
                c.set(x, y, PL_HI)
    c.rect(0, 9, T, 4, BM)
    c.hline(0, 9, T, BM_HI)
    c.hline(0, 12, T, BM_SH)
    c.rect(0, 38, T, 22, BM)
    for x in range(0, T, 8):  # wainscot boards
        c.vline(x, 38, 22, BM_SH)
        c.vline(x + 1, 38, 22, BM_HI)
    c.hline(0, 38, T, BM_HI)
    c.rect(0, 60, T, 4, BM_SH)
    if kind == "post":
        c.rect(0, 2, 4, 58, BM)
        c.vline(0, 2, 58, BM_HI)
        c.vline(3, 2, 58, BM_SH)
    return c


def wall_cap(mask):
    c = Canvas(T, T)
    c.rect(0, 0, T, T, CAP)
    for y in range(3, T, 6):
        c.hline(0, y, T, rgb("#3a2c24"))
    if not mask & 1:
        c.hline(0, 0, T, CAP_EDGE)
        c.hline(0, 1, T, CAP_RIM)
    if not mask & 4:
        c.hline(0, T - 1, T, CAP_EDGE)
        c.hline(0, T - 2, T, CAP_RIM)
    if not mask & 8:
        c.vline(0, 0, T, CAP_EDGE)
        c.vline(1, 0, T, CAP_RIM)
    if not mask & 2:
        c.vline(T - 1, 0, T, CAP_EDGE)
        c.vline(T - 2, 0, T, CAP_RIM)
    return c


def wall_shadow(mask):
    c = Canvas(T, T)
    for y in range(T):
        for x in range(T):
            a = 0
            if mask & 1 and y < 12:
                a = max(a, int(110 * (1 - y / 12) ** 1.6))
            if mask & 2 and x < 7:
                a = max(a, int(70 * (1 - x / 7) ** 1.5))
            if mask & 4 and x > T - 8:
                a = max(a, int(70 * (1 - (T - 1 - x) / 7) ** 1.5))
            if a:
                c.replace(x, y, with_alpha(rgb("#1a0f0a"), a))
    return c


def _quilt(c, x0, y0, w, h, rumpled=False):
    for y in range(y0, y0 + h):
        for x in range(x0, x0 + w):
            patch = ((x - x0) // 7 + (y - y0) // 7) % 4
            col = QUILT[patch]
            if (x - x0) % 7 == 0 or (y - y0) % 7 == 0:
                col = mix(col, OUT, 0.35)
            c.set(x, y, col)
    c.hline(x0, y0, w, LINEN)
    c.hline(x0, y0 + 1, w, LINEN_SH)
    c.vline(x0 + w - 1, y0, h, mix(QUILT[0], OUT, 0.4))
    if rumpled:
        for i in range(3):
            c.line(x0 + 3 + i * 7, y0 + 4, x0 + 8 + i * 7, y0 + 10, mix(QUILT[1], OUT, 0.45))


def bed(state):
    """state: sleeping | startled | empty. 32x80 (bed body is the bottom 64 px)."""
    c = Canvas(32, 80)
    top = 16  # bed body starts here; space above is for the sitting-up hero
    c.rect(1, top, 30, 63, BM)
    c.rect(1, top, 30, 9, BM_SH)            # headboard
    c.hline(1, top, 30, BM_HI)
    c.rect(2, top + 2, 2, 6, BM_HI)
    c.rect(28, top + 2, 2, 6, BM_HI)
    c.rect(4, top + 9, 24, 52, LINEN_SH)     # mattress
    c.ellipse(16, top + 13, 10, 4, LINEN)    # pillow
    c.hline(8, top + 16, 16, LINEN_SH)
    if state == "sleeping":
        c.ascii(hero.HEAD_DOWN_ASLEEP, hero.PAL, 8, top + 1)
        _quilt(c, 3, top + 17, 26, 42)
    elif state == "startled":
        body = Canvas(32, 32)
        body.ascii(hero.TORSO_DOWN, hero.PAL, 9, 15)
        body.ascii(hero.ARM, hero.PAL, 7, 17)
        body.ascii(hero.ARM, hero.PAL, 21, 17)
        body.ascii(hero.HEAD_DOWN_STARTLED, hero.PAL, 8, 0)
        c.blit(body, 0, top - 6)
        _quilt(c, 3, top + 22, 26, 37, rumpled=True)
    else:
        c.ellipse(16, top + 13, 6, 2, LINEN_SH)  # dent in the pillow
        _quilt(c, 3, top + 30, 26, 29, rumpled=True)
    c.rect(1, top + 59, 30, 5, BM)           # footboard
    c.hline(1, top + 59, 30, BM_HI)
    c.outline(OUT)
    return c


def nightstand():
    c = Canvas(20, 26)
    c.rect(2, 10, 16, 14, BM)
    c.rect(1, 8, 18, 3, BM_HI)
    c.rect(4, 13, 12, 5, BM_SH)
    c.set(10, 15, rgb("#d4ad55"))
    c.rect(3, 24, 2, 2, BM_SH)
    c.rect(15, 24, 2, 2, BM_SH)
    # candle in a brass holder
    c.ellipse(10, 8, 4, 1.5, rgb("#c9a24a"))
    c.rect(9, 2, 3, 6, rgb("#f4ead2"))
    c.vline(11, 2, 6, rgb("#d8caa8"))
    c.outline(OUT)
    return c


def candle_flame(frame):
    c = Canvas(5, 7)
    core, mid, outer = rgb("#fff6c8"), rgb("#ffcf5a"), rgb("#ff8f2e")
    sway = (0, 1)[frame]
    c.vline(2, 2, 4, mid)
    c.set(2 + sway, 1, outer)
    c.set(2, 4, core)
    c.set(2, 5, core)
    c.set(1, 4, outer)
    c.set(3, 4, outer)
    c.set(2, 6, rgb("#5a4a3a"))
    return c


def window():
    c = Canvas(36, 36)
    c.rect(2, 2, 32, 30, BM)
    night_top, night_bottom = rgb("#1b2546"), rgb("#33467a")
    for y in range(5, 29):
        c.hline(5, y, 26, mix(night_top, night_bottom, (y - 5) / 24))
    for (x, y) in ((8, 8), (14, 12), (26, 7), (22, 17), (10, 20), (28, 22)):
        c.set(x, y, rgb("#fff6d8"))
    c.ellipse(24, 12, 4, 4, rgb("#fff6c8"))
    c.ellipse(26, 11, 3.5, 3.5, mix(night_top, night_bottom, 0.3))
    c.vline(17, 5, 24, BM_SH)
    c.vline(18, 5, 24, BM_HI)
    c.hline(5, 16, 26, BM_SH)
    c.hline(5, 17, 26, BM_HI)
    c.rect(1, 29, 34, 4, BM_HI)  # sill
    c.hline(1, 32, 34, BM_SH)
    for x0 in (0, 31):  # curtains
        for y in range(1, 30):
            c.hline(x0, y, 5, QUILT[0] if (y // 3) % 2 else mix(QUILT[0], OUT, 0.25))
    c.outline(OUT)
    return c


def fireplace(frame):
    rnd = random.Random(frame)
    c = Canvas(48, 64)
    for y in range(4, 62, 6):  # stone surround
        offset = 0 if (y // 6) % 2 else 6
        for x in range(1 - offset, 47, 12):
            x0 = max(1, x)
            c.rect(x0, y, min(11, 47 - x0), 5, ST)
            c.hline(x0, y, min(11, 47 - x0), ST_HI)
            c.hline(x0, y + 4, min(11, 47 - x0), ST_SH)
    c.rect(0, 0, 48, 5, BM)
    c.hline(0, 0, 48, BM_HI)
    c.rect(10, 26, 28, 36, rgb("#1a1210"))   # firebox
    c.rect(10, 26, 28, 3, rgb("#2e221c"))
    ember = [rgb("#ff6a2a"), rgb("#ffb84a"), rgb("#a8321c")]
    c.line(14, 58, 33, 55, BM)
    c.line(14, 55, 33, 58, BM_SH)
    for _ in range(18):
        x, y = rnd.randrange(13, 35), rnd.randrange(53, 61)
        c.set(x, y, ember[rnd.randrange(3)])
    for _ in range(4 + frame):
        x, y = rnd.randrange(16, 32), rnd.randrange(46, 54)
        c.set(x, y, ember[0])
    c.rect(4, 61, 40, 3, ST_SH)
    c.outline(OUT)
    return c


def table():
    c = Canvas(48, 40)
    c.rect(2, 10, 44, 14, rgb("#a8744a"))
    for y in (13, 17, 21):
        c.hline(2, y, 44, rgb("#8a5c38"))
    c.hline(2, 10, 44, rgb("#c48e5e"))
    c.rect(2, 24, 44, 4, BM)
    for x in (3, 41):
        c.rect(x, 28, 4, 10, BM_SH)
    # bread, jug and a mug
    c.ellipse(13, 15, 6, 3, rgb("#c98a3a"))
    c.ellipse(12, 14, 4, 1.5, rgb("#e2a95a"))
    c.rect(26, 6, 7, 11, rgb("#8f9aa8"))
    c.vline(26, 6, 11, rgb("#b8c2cc"))
    c.rect(33, 9, 2, 4, rgb("#8f9aa8"))
    c.rect(37, 13, 4, 5, rgb("#c9a24a"))
    c.outline(OUT)
    return c


def chair():
    c = Canvas(20, 30)
    c.rect(3, 2, 14, 12, BM)
    c.hline(3, 2, 14, BM_HI)
    c.rect(5, 5, 10, 6, BM_SH)
    c.rect(2, 14, 16, 5, rgb("#a8744a"))
    c.hline(2, 14, 16, rgb("#c48e5e"))
    for x in (3, 15):
        c.rect(x, 19, 2, 10, BM_SH)
    c.outline(OUT)
    return c


def chest():
    c = Canvas(32, 26)
    c.rect(2, 6, 28, 18, BM)
    c.rect(2, 4, 28, 6, BM_HI)
    c.hline(2, 4, 28, rgb("#9a6c4a"))
    iron = rgb("#5e5a63")
    for x in (6, 24):
        c.vline(x, 4, 20, iron)
        c.vline(x + 1, 4, 20, rgb("#7e7a84"))
    c.rect(14, 10, 4, 5, rgb("#d4ad55"))
    c.set(15, 12, OUT)
    c.outline(OUT)
    return c


def rug():
    c = Canvas(64, 40)
    rings = [rgb("#7e2e2a"), rgb("#c98a3a"), rgb("#a8413a"), rgb("#e0c48a"), rgb("#5a7a4a"), rgb("#a8413a")]
    for i, col in enumerate(rings):
        c.ellipse(32, 20, 31 - i * 4.5, 19 - i * 2.8, col)
    return c


def shelf():
    c = Canvas(48, 26)
    c.rect(0, 18, 48, 4, BM)
    c.hline(0, 18, 48, BM_HI)
    for x in (6, 40):
        c.rect(x, 22, 3, 4, BM_SH)
    items = [(4, 8, rgb("#8f9aa8")), (12, 10, rgb("#c98a3a")), (20, 7, rgb("#5a7a4a")), (28, 9, rgb("#a8413a")), (37, 8, rgb("#e0c48a"))]
    for x, h, col in items:
        c.rect(x, 18 - h, 6, h, col)
        c.vline(x, 18 - h, h, mix(col, rgb("#ffffff"), 0.3))
        c.hline(x + 1, 17 - h, 4, mix(col, OUT, 0.3))
    c.outline(OUT)
    return c


def door():
    c = Canvas(32, 60)
    c.rect(2, 2, 28, 58, BM_SH)
    for x in range(4, 28, 6):
        c.rect(x, 4, 5, 56, BM)
        c.vline(x, 4, 56, BM_HI)
    iron = rgb("#5e5a63")
    for y in (12, 44):
        c.rect(4, y, 22, 3, iron)
        c.hline(4, y, 22, rgb("#7e7a84"))
    c.ellipse(24, 32, 2.5, 2.5, rgb("#d4ad55"))
    c.set(24, 32, OUT)
    c.outline(OUT)
    return c


def boots():
    c = Canvas(16, 10)
    for x0 in (1, 9):
        c.rect(x0, 1, 5, 6, hero.PAL["F"])
        c.rect(x0, 6, 6, 2, hero.PAL["F"])
        c.vline(x0, 1, 6, hero.PAL["f"])
    c.outline(OUT)
    return c
