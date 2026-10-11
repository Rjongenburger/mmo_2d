"""The mage (Aldric): 32x64 frames. idle0..idle3 (wind-blown), cast0..cast1 (staff raised)."""
from pixel import Canvas, rgb, sheet, with_alpha

FRAME_W, FRAME_H = 32, 64
PIVOT = (16, 2)
COLUMNS = ("idle0", "idle1", "idle2", "idle3", "cast0", "cast1")

O = rgb("#2b2433")
R0, R1, R2, R3 = rgb("#6d6ad0"), rgb("#4b48a8"), rgb("#35338a"), rgb("#24226a")
Y0, Y1, Y2 = rgb("#ffe08a"), rgb("#d9a93e"), rgb("#9c7424")
W0, W1, W2 = rgb("#ffffff"), rgb("#e4e8ef"), rgb("#b4bccb")
S0, S1 = rgb("#f0c49c"), rgb("#cf9a74")
T0, T1, T2 = rgb("#a3714a"), rgb("#7a5232"), rgb("#4f3420")
K0, K1, K2, K3 = rgb("#f0ffff"), rgb("#8ff0ff"), rgb("#3cc0e0"), rgb("#1d7fa8")


def _crystal(c, cx, top, bright=False):
    widths = (1, 3, 3, 5, 3, 3, 1)
    for i, w in enumerate(widths):
        y = top + i
        for j in range(w):
            x = cx - w // 2 + j
            col = K1 if j < w // 2 else K2
            if i <= 1 or (bright and j == 0):
                col = K0
            if i >= 5:
                col = K3 if j > 0 else K2
            c.set(x, y, col)


def _staff(c, top, bottom):
    c.rect(6, top, 2, bottom - top, T1)
    c.vline(6, top, bottom - top, T0)
    c.set(5, top - 1, T2)
    c.set(8, top - 1, T2)
    c.set(5, top - 2, T2)
    c.set(8, top - 2, T2)
    c.rect(6, top - 1, 2, 2, T2)


def _hat(c, tip_dx):
    for y in range(5, 19):
        t = (y - 5) / 13
        w = max(2, round(2 + t * 11))
        cx = 16 + (19 - y) * 0.22
        x0 = round(cx - w / 2)
        c.hline(x0, y, w, R1)
        c.hline(x0, y, max(1, w // 4), R0)
        c.hline(x0 + w - max(1, w // 3), y, max(1, w // 3), R2)
    # wind-bent tip
    for i, (x, y) in enumerate(((19, 4), (20, 3), (21, 3), (22, 4), (23, 5))):
        c.set(x + (tip_dx if i >= 2 else 0), y, R2 if i >= 3 else R1)
    c.hline(10, 15, 13, Y1)
    c.hline(10, 16, 13, Y2)
    c.hline(10, 15, 3, Y0)
    c.set(17, 10, Y0)
    c.set(16, 10, Y1)
    c.set(18, 10, Y1)
    c.set(17, 9, Y1)
    c.set(17, 11, Y1)
    # brim
    c.ellipse(16, 19.5, 11, 2.6, R2)
    c.ellipse(16, 18.8, 10, 1.9, R1)
    c.hline(7, 18, 6, R0)


def _face(c):
    c.ellipse(16, 24.5, 4.6, 4.2, S0)
    c.hline(12, 21, 9, S1)
    c.hline(12, 22, 9, S1)
    c.hline(12, 23, 3, W1)
    c.hline(18, 23, 3, W1)
    c.set(14, 24, O)
    c.set(18, 24, O)
    c.set(16, 26, S1)
    c.set(16, 25, S1)


def _beard(c, sway):
    c.ellipse(16, 33, 6.2, 8.5, W1)
    c.ellipse(15, 31.5, 5, 6.5, W0)
    for y in range(36, 42):
        c.set(20 + (y - 36) // 3, y, W2)
    c.vline(13, 34, 5, W2)
    c.vline(17, 36, 5, W2)
    # tapering tip pushed by the wind
    for i, y in enumerate(range(41, 46)):
        x = 16 + sway + i // 2
        c.hline(x - 2 + i // 2, y, max(1, 4 - i), W1 if i < 3 else W2)
    # moustache
    c.hline(12, 27, 9, W0)
    c.hline(11, 28, 4, W1)
    c.hline(18, 28, 4, W1)
    c.set(15, 28, O)
    c.set(16, 28, O)


def _robe(c, phase):
    for y in range(30, 61):
        t = (y - 30) / 30
        xl, xr = round(10 - t * 3.6), round(22 + t * 3.6)
        if y >= 52:  # hem flutters to the east
            xr += (1 + ((y + phase) % 3 == 0)) if (y + phase) % 2 else 0
        c.hline(xl, y, xr - xl + 1, R1)
        c.hline(xl, y, 2, R0)
        c.hline(xr - 2, y, 3, R2)
    c.vline(16, 42, 16, Y1)
    c.vline(15, 42, 16, R3)
    for y in (58, 59, 60):
        t = (y - 30) / 30
        xl, xr = round(10 - t * 3.6), round(22 + t * 3.6)
        c.hline(xl, y, xr - xl + 1, Y0 if y == 58 else Y1)
    c.hline(11, 41, 11, Y1)
    c.set(13, 42, Y1)
    c.set(13, 43, Y2)


def _sleeves(c, casting):
    # left sleeve reaches for the staff
    hand_y = 29 if casting else 35
    for y in range(31, 39):
        c.hline(7 + max(0, 38 - y - 4), y, 6 - max(0, 38 - y - 4) // 2, R1)
    c.rect(8, hand_y - 1, 2, 2, Y1)
    c.rect(6, hand_y, 3, 2, S0)
    if casting:
        for y in range(28, 34):
            c.hline(20 + (34 - y) // 2, y, 4, R1)
            c.set(23 + (34 - y) // 2, y, R2)
        c.rect(24, 25, 3, 3, S0)
        c.set(26, 25, S1)
    else:
        c.rect(21, 31, 4, 13, R1)
        c.vline(24, 31, 13, R2)
        c.hline(21, 43, 4, Y1)
        c.rect(22, 44, 2, 2, S0)


def frame(idx):
    casting = idx >= 4
    c = Canvas(FRAME_W, FRAME_H)
    staff_top = 10 if casting else 12
    _staff(c, staff_top, 61 if not casting else 55)
    _robe(c, idx)
    _sleeves(c, casting)
    _beard(c, (0, 1, 2, 1)[idx % 4])
    _face(c)
    _hat(c, (0, 1, 1, 0)[idx % 4])
    c.outline(O)
    _crystal(c, 7, staff_top - 9, bright=casting)
    # crystal glow halo
    glow = with_alpha(K1, 70 if not casting else 120)
    cy = staff_top - 6
    for dx, dy in ((-3, 0), (3, 0), (0, -5), (0, 4), (-2, -3), (2, -3), (-2, 3), (2, 3)):
        if c.get(7 + dx, cy + dy)[3] == 0:
            c.set(7 + dx, cy + dy, glow)
    spark = ((10, cy - 4), (4, cy - 2), (9, cy + 3), (5, cy - 5))[idx % 4]
    c.set(*spark, K0)
    if casting:
        for x, y in ((26, 21), (29, 24), (24, 22), (28, 28)):
            c.set(x, y, K0 if (x + y + idx) % 2 else K1)
    return c


def build_sheet():
    return sheet([frame(i) for i in range(len(COLUMNS))], len(COLUMNS))


def frame_names(prefix="Mage"):
    return [f"{prefix}_{col}" for col in COLUMNS]


def head_portrait():
    return frame(0).crop(4, 14, 24, 24).scaled(2)
