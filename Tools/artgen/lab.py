"""White sci-fi lab tiles and props (32 px per tile, 3/4 top-down)."""
import random

from pixel import Canvas, rgb, with_alpha

T = 32

OUT = rgb("#2b3040")
W0 = rgb("#ffffff")
W1 = rgb("#f3f6f9")
W2 = rgb("#e6ebf0")
W3 = rgb("#d5dce5")
G1 = rgb("#c0c9d4")
G2 = rgb("#9ba7b6")
G3 = rgb("#747f91")
G4 = rgb("#525c6e")
G5 = rgb("#3b4354")
G6 = rgb("#2b3040")
VOID = rgb("#1a1e27")

TEAL0, TEAL1, TEAL2, TEAL3 = rgb("#8ff0e6"), rgb("#3cc8bc"), rgb("#24978f"), rgb("#17625e")
CYAN0, CYAN1, CYAN2 = rgb("#d6fcff"), rgb("#7ae8ff"), rgb("#36b9e6")
GRN0, GRN1, GRN2, GRN3, GRN4 = rgb("#d2ffe2"), rgb("#86f7b0"), rgb("#45d283"), rgb("#249459"), rgb("#145a37")
YEL0, YEL1, YEL2 = rgb("#ffd75e"), rgb("#e0ae2c"), rgb("#a37a12")
RED0, RED1, RED2 = rgb("#ff7b7b"), rgb("#d9434b"), rgb("#9c2a35")
BLU0, BLU1, BLU2, BLU3, BLU4 = rgb("#b8e4ff"), rgb("#5ab4ff"), rgb("#2f7fe0"), rgb("#1b4fa0"), rgb("#0f2c5c")
PNK0, PNK1 = rgb("#ffb3dd"), rgb("#e65fa8")
LEAF0, LEAF1, LEAF2, LEAF3 = rgb("#a5df73"), rgb("#6cbd52"), rgb("#43914a"), rgb("#2a6338")
AMB0, AMB1 = rgb("#f2b45a"), rgb("#b9762a")


# ---------------------------------------------------------------- floor
def floor_tile(kind, seed=0):
    rnd = random.Random(seed)
    c = Canvas(T, T)
    base, hi, shade, grout = W1, W0, W2, W3
    if kind == "zone":
        base, hi, shade, grout = rgb("#e4edf6"), rgb("#f4f9ff"), rgb("#d6e2ee"), rgb("#c3d1df")
    for sy in (0, 16):
        for sx in (0, 16):
            c.rect(sx, sy, 16, 16, base)
            c.hline(sx, sy, 15, hi)
            c.vline(sx, sy, 15, hi)
            c.hline(sx + 1, sy + 14, 14, shade)
            c.vline(sx + 14, sy + 1, 14, shade)
            c.hline(sx, sy + 15, 16, grout)
            c.vline(sx + 15, sy, 16, grout)
    speckles = {"clean": 3, "speck": 10, "scuff": 4, "zone": 4}.get(kind, 2)
    for _ in range(speckles):
        x, y = rnd.randrange(1, 31), rnd.randrange(1, 31)
        if x % 16 not in (0, 15) and y % 16 not in (0, 15):
            c.set(x, y, shade)
    if kind == "scuff":
        cx, cy = rnd.randrange(8, 24), rnd.randrange(8, 24)
        for i in range(9):
            c.set(cx + i - 4, cy + (i * i) // 12 - 1, with_alpha(G1, 90))
    elif kind == "drain":
        c.ellipse(16, 16, 7, 7, G2)
        c.ellipse(16, 16, 6, 6, G1)
        c.ellipse(16, 16, 5, 5, G4)
        for i in range(-3, 4, 2):
            c.hline(12, 16 + i, 9, G5)
        c.ellipse(15, 15, 1.5, 1.5, with_alpha(W0, 120))
    elif kind == "vent":
        c.rect(5, 9, 22, 14, G2)
        c.rect(6, 10, 20, 12, G4)
        for y in range(11, 21, 2):
            c.hline(7, y, 18, G3)
        c.hline(5, 9, 22, G1)
    return c


def wall_shadow(mask):
    """Ambient-occlusion overlay for floor cells next to walls. mask: 1=wall face above, 2=cap left, 4=cap right."""
    c = Canvas(T, T)
    alpha = [[0] * T for _ in range(T)]
    for y in range(T):
        for x in range(T):
            a = 0
            if mask & 1 and y < 12:
                a = max(a, int(85 * (1 - y / 12) ** 1.6))
            if mask & 2 and x < 7:
                a = max(a, int(55 * (1 - x / 7) ** 1.5))
            if mask & 4 and x > T - 8:
                a = max(a, int(55 * (1 - (T - 1 - x) / 7) ** 1.5))
            alpha[y][x] = a
    for y in range(T):
        for x in range(T):
            if alpha[y][x]:
                c.replace(x, y, with_alpha(rgb("#3a4a66"), alpha[y][x]))
    return c


# ---------------------------------------------------------------- walls
def wall_face(kind):
    """North wall face, two tiles tall (32x64): returns canvas (sliced into upper/lower by the importer)."""
    c = Canvas(T, 2 * T)
    # cap overhang shadow
    c.hline(0, 0, T, G4)
    c.hline(0, 1, T, G2)
    c.hline(0, 2, T, G1)
    # upper panel
    c.rect(0, 3, T, 33, W1)
    c.rect(0, 3, T, 3, W2)
    # teal stripe
    c.hline(0, 36, T, TEAL0)
    c.rect(0, 37, T, 2, TEAL1)
    c.hline(0, 39, T, TEAL2)
    # lower panel
    c.rect(0, 40, T, 16, W2)
    c.hline(0, 40, T, W3)
    # panel seams
    for y0, h in ((3, 33), (40, 16)):
        c.vline(0, y0, h, W3)
        c.vline(1, y0, h, W0)
    # baseboard
    c.hline(0, 56, T, G1)
    c.rect(0, 57, T, 4, G2)
    c.hline(0, 61, T, G3)
    c.rect(0, 62, T, 2, G4)
    if kind == "outlet":
        c.rect(20, 45, 7, 7, W0)
        c.frame(20, 45, 7, 7, G2)
        c.set(22, 48, G4)
        c.set(24, 48, G4)
    elif kind == "vent":
        c.rect(8, 10, 16, 12, G1)
        c.frame(8, 10, 16, 12, G2)
        for y in range(12, 21, 2):
            c.hline(10, y, 12, G3)
    elif kind == "pipe":
        c.rect(24, 3, 4, 53, G1)
        c.vline(24, 3, 53, W0)
        c.vline(27, 3, 53, G2)
        for y in (12, 30, 48):
            c.rect(23, y, 6, 2, G3)
    return c


def wall_cap(mask):
    """Top of a wall seen from above. mask bits: 1=N, 2=E, 4=S, 8=W neighbour is also wall."""
    c = Canvas(T, T)
    c.rect(0, 0, T, T, G5)
    # subtle panel pattern
    for y in range(0, T, 8):
        c.hline(0, y, T, rgb("#404a5c"))
    for y in range(4, T, 8):
        for x in range((y // 8) % 2 * 8, T, 16):
            c.set(x, y, rgb("#353d4d"))
    rim, edge = rgb("#6a768c"), rgb("#1f232c")
    if not mask & 1:
        c.hline(0, 0, T, edge)
        c.hline(0, 1, T, rim)
    if not mask & 4:
        c.hline(0, T - 1, T, edge)
        c.hline(0, T - 2, T, rim)
    if not mask & 8:
        c.vline(0, 0, T, edge)
        c.vline(1, 1 if not mask & 1 else 0, T - (1 if not mask & 1 else 0) - (1 if not mask & 4 else 0), rim)
    if not mask & 2:
        c.vline(T - 1, 0, T, edge)
        c.vline(T - 2, 1 if not mask & 1 else 0, T - (1 if not mask & 1 else 0) - (1 if not mask & 4 else 0), rim)
    return c


# ---------------------------------------------------------------- shared helpers
def soft_shadow():
    c = Canvas(32, 16)
    c.soft_ellipse(16, 8, 15.5, 7.5, rgb("#1e2a40", 110), falloff=0.6)
    return c


def _monitor(c, x, y, w=22, h=15, seed=0):
    rnd = random.Random(seed)
    c.rect(x, y, w, h, G6)
    c.rect(x + 2, y + 2, w - 4, h - 4, BLU3)
    c.rect(x + 2, y + 2, w - 4, 2, BLU2)
    for row in range(y + 5, y + h - 3, 2):
        indent = rnd.choice((0, 2, 4))
        length = rnd.randrange(4, w - 8 - indent)
        col = rnd.choice((BLU0, CYAN1, TEAL0, BLU1))
        c.hline(x + 3 + indent, row, length, col)
    c.set(x + w - 3, y + h - 2, GRN1)
    c.rect(x + w // 2 - 2, y + h, 4, 3, G4)
    c.rect(x + w // 2 - 5, y + h + 3, 10, 2, G3)


def _keyboard(c, x, y):
    c.rect(x, y, 16, 5, G1)
    c.hline(x, y, 16, W1)
    for yy in (y + 1, y + 3):
        for xx in range(x + 1, x + 15, 2):
            c.set(xx, yy, G3)
    c.hline(x, y + 5, 16, G3)


def _flask(c, x, y, liquid, light):
    # erlenmeyer: 7 wide, 10 tall
    glass = with_alpha(W0, 200)
    c.rect(x + 2, y, 3, 4, glass)
    c.hline(x + 1, y, 5, G2)
    for i in range(6):
        c.hline(x + 2 - i // 2, y + 4 + i, 3 + (i // 2) * 2, glass)
    for i in range(3, 6):
        c.hline(x + 2 - i // 2, y + 4 + i, 3 + (i // 2) * 2, liquid)
    c.hline(x + 1, y + 7, 5, light)
    c.set(x + 3, y + 1, W0)


def _tube_rack(c, x, y, colours):
    c.rect(x, y + 5, 3 * len(colours) + 2, 4, G1)
    c.hline(x, y + 5, 3 * len(colours) + 2, W0)
    for i, col in enumerate(colours):
        tx = x + 1 + i * 3
        c.rect(tx, y, 2, 7, with_alpha(W0, 210))
        c.rect(tx, y + 3, 2, 4, col)
    c.hline(x, y + 9, 3 * len(colours) + 2, G3)


def _microscope(c, x, y):
    c.rect(x, y + 13, 12, 3, G4)
    c.hline(x, y + 13, 12, G3)
    c.rect(x + 7, y + 3, 3, 11, W1)
    c.vline(x + 9, y + 3, 11, G1)
    c.rect(x + 2, y + 9, 7, 2, G2)
    c.rect(x + 3, y + 1, 3, 8, G5)
    c.rect(x + 3, y, 3, 2, G6)
    c.set(x + 4, y + 2, G3)
    c.rect(x + 4, y + 11, 2, 2, CYAN1)


def _mug(c, x, y, col):
    c.rect(x, y, 4, 5, col)
    c.hline(x, y, 4, W0)
    c.set(x + 4, y + 2, col)
    c.set(x + 4, y + 3, col)


def _papers(c, x, y):
    c.rect(x, y, 9, 7, W0)
    c.rect(x + 1, y + 1, 9, 7, W1)
    for yy in range(y + 2, y + 7, 2):
        c.hline(x + 2, yy, 6, G1)


# ---------------------------------------------------------------- props (floor)
def bench(items):
    """64x48 lab bench. items: 'computer' | 'microscope' | 'chem'."""
    c = Canvas(64, 48)
    # top slab
    c.rect(1, 16, 62, 12, W1)
    c.hline(1, 16, 62, W0)
    c.hline(1, 27, 62, W0)
    c.rect(1, 28, 62, 2, G1)
    # cabinets
    c.rect(1, 30, 62, 14, W2)
    for dx in (16, 32, 48):
        c.vline(dx, 30, 14, G1)
        c.vline(dx + 1, 30, 14, W1)
    for dx in (12, 20, 44, 52):
        c.rect(dx, 33, 1, 4, G3)
    c.rect(1, 44, 62, 2, G4)
    if items == "computer":
        _monitor(c, 8, 1, seed=3)
        _keyboard(c, 11, 21)
        _papers(c, 38, 18)
        _mug(c, 52, 18, TEAL1)
    elif items == "microscope":
        _microscope(c, 8, 8)
        _papers(c, 24, 19)
        _flask(c, 40, 13, GRN2, GRN0)
        _tube_rack(c, 49, 14, [PNK1, CYAN2, GRN2])
    else:
        _flask(c, 6, 12, CYAN2, CYAN0)
        _flask(c, 15, 14, PNK1, PNK0)
        _tube_rack(c, 25, 13, [GRN2, YEL1, PNK1, CYAN2])
        _flask(c, 43, 12, GRN2, GRN0)
        c.rect(52, 15, 7, 8, G4)  # small centrifuge
        c.ellipse(55.5, 17.5, 2.5, 1.5, G2)
        c.set(57, 21, GRN1)
    c.outline(OUT)
    return c


def server_rack(frame):
    rnd = random.Random(42)
    c = Canvas(32, 64)
    c.rect(2, 4, 28, 6, G4)
    c.hline(2, 4, 28, G3)
    c.rect(2, 10, 28, 50, G5)
    c.vline(2, 10, 50, G4)
    c.vline(29, 10, 50, G6)
    for i, y in enumerate(range(12, 56, 7)):
        c.rect(4, y, 24, 6, rgb("#454f63"))
        c.hline(4, y, 24, rgb("#566179"))
        c.hline(4, y + 5, 24, G6)
        for x in range(6, 16, 2):
            c.set(x, y + 3, G6)
        for j in range(3):
            on = rnd.random() < 0.65
            if frame == 1 and rnd.random() < 0.4:
                on = not on
            col = (GRN1, CYAN1, YEL0)[(i + j) % 3] if on else G4
            c.set(20 + j * 3, y + 2, col)
    c.rect(2, 58, 28, 2, G6)
    c.outline(OUT)
    return c


def tank(frame):
    """Specimen tank 32x72 with animated bubbles (4 frames)."""
    c = Canvas(32, 72)
    # top cap
    c.rect(6, 0, 3, 5, G3)
    c.rect(22, 0, 3, 5, G3)
    c.rect(3, 5, 26, 9, G2)
    c.ellipse(16, 6, 13, 3, G1)
    c.ellipse(16, 5.5, 10, 2, W1)
    c.hline(3, 12, 26, G3)
    c.hline(3, 13, 26, G4)
    # glass + liquid
    top, bottom = 14, 56
    for y in range(top, bottom):
        t = (y - top) / (bottom - top)
        col = GRN1 if t < 0.35 else GRN2
        c.hline(4, y, 24, col)
        c.hline(4, y, 3, GRN0 if t < 0.5 else GRN1)
        c.hline(23, y, 5, GRN3)
        c.hline(26, y, 2, GRN4)
    c.hline(4, top, 24, GRN0)
    c.hline(4, top + 1, 24, with_alpha(W0, 140))
    # specimen silhouette
    c.ellipse(16, 36, 5, 7, with_alpha(GRN4, 200))
    c.ellipse(15, 28, 3.5, 3.5, with_alpha(GRN4, 200))
    c.rect(12, 41, 2, 7, with_alpha(GRN4, 200))
    c.rect(18, 41, 2, 6, with_alpha(GRN4, 200))
    c.set(14, 27, with_alpha(GRN1, 220))
    # glass highlight
    c.vline(7, top + 2, bottom - top - 4, with_alpha(W0, 170))
    c.vline(8, top + 4, bottom - top - 12, with_alpha(W0, 90))
    # bubbles
    for bx, phase in ((10, 0), (19, 13), (14, 26), (22, 7)):
        by = bottom - 4 - ((frame * 10 + phase * 3) % (bottom - top - 6))
        c.set(bx, by, GRN0)
        c.set(bx + 1, by - 6, with_alpha(W0, 200))
    # base
    c.rect(1, 56, 30, 14, G3)
    c.hline(1, 56, 30, G1)
    c.hline(1, 57, 30, G2)
    c.rect(3, 61, 26, 2, TEAL1)
    c.hline(3, 61, 26, TEAL0)
    c.rect(1, 67, 30, 3, G4)
    c.outline(OUT)
    return c


def shelf():
    rnd = random.Random(7)
    c = Canvas(64, 64)
    c.rect(2, 2, 60, 4, W2)
    c.hline(2, 2, 60, W0)
    c.rect(4, 6, 56, 54, W3)
    for y in (6, 24, 42):
        # items on the shelf (sitting on y+16)
        x = 6
        while x < 56:
            kind = rnd.choice(("bottle", "box", "binder", "binder", "jar"))
            base = y + 16
            if kind == "bottle":
                col = rnd.choice((AMB0, BLU1, GRN2, PNK1))
                c.rect(x, base - 9, 4, 9, col)
                c.rect(x + 1, base - 12, 2, 3, col)
                c.set(x + 1, base - 12, RED1)
                c.vline(x, base - 8, 6, with_alpha(W0, 150))
                x += 6
            elif kind == "box":
                c.rect(x, base - 8, 9, 8, W1)
                c.hline(x, base - 8, 9, W0)
                c.rect(x + 2, base - 5, 5, 2, rnd.choice((TEAL1, YEL1, RED1)))
                c.vline(x + 8, base - 8, 8, G1)
                x += 10
            elif kind == "binder":
                col = rnd.choice((TEAL2, YEL1, RED1, BLU2))
                c.rect(x, base - 13, 3, 13, col)
                c.rect(x, base - 10, 3, 2, W1)
                x += 3 + rnd.choice((0, 0, 1))
            else:
                c.rect(x, base - 6, 5, 6, with_alpha(W0, 220))
                c.rect(x, base - 4, 5, 4, rnd.choice((GRN2, CYAN2, YEL1)))
                c.rect(x, base - 7, 5, 1, G3)
                x += 6
        c.rect(4, y + 16, 56, 2, W1)
        c.hline(4, y + 16, 56, W0)
        c.hline(4, y + 18, 56, G1)
    c.rect(2, 2, 2, 60, G1)
    c.rect(60, 2, 2, 60, G1)
    c.vline(2, 2, 60, W1)
    c.outline(OUT)
    return c


def plant():
    c = Canvas(32, 48)
    leaves = [
        (16, 14, 6, 9, LEAF2), (9, 20, 7, 5, LEAF2), (23, 19, 7, 5, LEAF2),
        (12, 12, 5, 7, LEAF1), (21, 12, 5, 7, LEAF1), (16, 6, 4, 6, LEAF1),
        (8, 24, 5, 3, LEAF1), (25, 24, 5, 3, LEAF1), (16, 22, 5, 5, LEAF3),
    ]
    for cx, cy, rx, ry, col in leaves:
        c.ellipse(cx, cy, rx, ry, col)
    for cx, cy in ((11, 10), (20, 9), (15, 4), (7, 18), (26, 17)):
        c.set(cx, cy, LEAF0)
        c.set(cx + 1, cy, LEAF0)
        c.set(cx, cy + 1, LEAF0)
    c.line(16, 26, 16, 12, LEAF3)
    c.line(16, 24, 9, 19, LEAF3)
    c.line(16, 24, 23, 18, LEAF3)
    # pot
    for i, y in enumerate(range(28, 44)):
        inset = i // 5
        c.hline(9 + inset, y, 14 - inset * 2, W1)
        c.set(9 + inset, y, W0)
        c.set(22 - inset, y, G1)
        c.set(21 - inset, y, W2)
    c.rect(8, 27, 16, 3, W0)
    c.hline(8, 29, 16, G1)
    c.rect(12, 36, 8, 2, TEAL1)
    c.outline(OUT)
    return c


def chair():
    c = Canvas(32, 32)
    # base + wheels
    c.hline(8, 27, 16, G5)
    c.line(9, 27, 6, 29, G5)
    c.line(23, 27, 26, 29, G5)
    for x in (5, 15, 26):
        c.rect(x, 29, 2, 2, G6)
    c.rect(15, 21, 2, 7, G3)
    # seat
    c.rect(7, 17, 18, 5, TEAL2)
    c.hline(7, 17, 18, TEAL1)
    c.hline(7, 21, 18, TEAL3)
    # backrest (seen from behind)
    c.rect(8, 3, 16, 14, TEAL2)
    c.rect(9, 4, 14, 2, TEAL1)
    c.vline(23, 4, 13, TEAL3)
    c.rect(14, 14, 4, 4, G4)
    c.outline(OUT)
    return c


def water_cooler():
    c = Canvas(32, 48)
    # jug
    c.rect(10, 2, 12, 16, rgb("#a8dcff", 220))
    c.rect(10, 7, 12, 11, rgb("#5fb4f0", 230))
    c.hline(10, 7, 12, BLU0)
    c.vline(11, 3, 14, with_alpha(W0, 180))
    c.rect(13, 0, 6, 2, BLU2)
    # body
    c.rect(8, 18, 16, 26, W1)
    c.hline(8, 18, 16, W0)
    c.vline(23, 18, 26, G1)
    c.rect(10, 24, 12, 7, W3)
    c.rect(12, 26, 2, 3, RED1)
    c.rect(18, 26, 2, 3, BLU2)
    c.rect(11, 31, 10, 2, G3)
    c.rect(8, 42, 16, 2, G2)
    c.outline(OUT)
    return c


def bin_():
    c = Canvas(16, 24)
    c.rect(2, 6, 12, 16, W2)
    c.vline(2, 6, 16, W0)
    c.vline(13, 6, 16, G1)
    c.rect(1, 3, 14, 4, G1)
    c.hline(1, 3, 14, W1)
    c.rect(5, 12, 6, 6, YEL1)
    c.set(7, 14, G6)
    c.set(8, 14, G6)
    c.set(7, 15, G6)
    c.set(8, 16, G6)
    c.outline(OUT)
    return c


def hazard_floor(w_tiles=4, h_tiles=2):
    """Metal grate platform with hazard-stripe border, drawn under the specimen tanks."""
    w, h = w_tiles * T, h_tiles * T
    c = Canvas(w, h)
    c.rect(0, 0, w, h, G4)
    for y in range(6, h - 6, 3):
        c.hline(6, y, w - 12, G5)
    for x in range(6, w - 6, 6):
        c.vline(x, 6, h - 12, G3)
    for y in range(h):
        for x in range(w):
            if x < 5 or x >= w - 5 or y < 5 or y >= h - 5:
                c.replace(x, y, YEL0 if ((x + y) // 4) % 2 == 0 else G6)
    c.frame(0, 0, w, h, OUT)
    return c


# ---------------------------------------------------------------- props (wall-mounted)
def whiteboard():
    c = Canvas(64, 40)
    c.rect(0, 0, 64, 36, G1)
    c.rect(2, 2, 60, 32, W0)
    # line chart
    pts = [(6, 28), (12, 22), (18, 25), (24, 14), (30, 18), (36, 8)]
    for (x0, y0), (x1, y1) in zip(pts, pts[1:]):
        c.line(x0, y0, x1, y1, BLU2)
    c.line(5, 6, 5, 30, G4)
    c.line(5, 30, 38, 30, G4)
    # formula squiggles
    for i, y in enumerate((7, 12, 17)):
        for x in range(42, 58 - i * 3):
            if (x + i) % 5 != 0:
                c.set(x, y + (1 if (x // 2) % 2 else 0), (RED1, GRN3, G4)[i])
    c.ellipse(48, 25, 6, 4, RED1)
    c.ellipse(48, 25, 5, 3, W0)
    c.rect(4, 35, 56, 3, G2)
    for x, col in ((10, RED1), (14, BLU2), (18, GRN3)):
        c.rect(x, 34, 3, 2, col)
    c.outline(OUT)
    return c


def wall_screen():
    c = Canvas(48, 32)
    c.rect(0, 0, 48, 30, G6)
    c.rect(2, 2, 44, 26, BLU4)
    for i, h in enumerate((6, 10, 8, 14, 12)):
        c.rect(5 + i * 4, 24 - h, 3, h, TEAL1 if i % 2 else CYAN2)
    pts = [(26, 22), (30, 16), (34, 18), (38, 9), (43, 12)]
    for (x0, y0), (x1, y1) in zip(pts, pts[1:]):
        c.line(x0, y0, x1, y1, GRN1)
    c.hline(5, 5, 14, BLU1)
    c.hline(5, 7, 9, BLU2)
    c.set(43, 4, RED0)
    c.rect(20, 30, 8, 2, G4)
    c.outline(OUT)
    return c


def door():
    c = Canvas(64, 64)
    c.rect(2, 4, 60, 60, TEAL2)
    c.hline(2, 4, 60, TEAL1)
    c.rect(6, 9, 52, 55, G1)
    for x0 in (6, 33):
        c.rect(x0, 9, 25, 55, W2)
        c.vline(x0, 9, 55, W0)
        c.rect(x0 + 8, 16, 9, 24, rgb("#bfe9f2"))
        c.vline(x0 + 9, 17, 22, with_alpha(W0, 200))
        c.frame(x0 + 8, 16, 9, 24, G2)
        c.rect(x0, 52, 25, 6, YEL0)
        for x in range(x0, x0 + 25):
            if ((x + 52) // 3) % 2:
                c.vline(x, 52, 6, G6)
    c.vline(31, 9, 55, G4)
    c.vline(32, 9, 55, G6)
    c.rect(28, 0, 8, 4, G5)
    c.rect(30, 1, 4, 2, GRN1)
    c.outline(OUT)
    return c


def clock():
    c = Canvas(16, 16)
    c.ellipse(8, 8, 7, 7, G4)
    c.ellipse(8, 8, 6, 6, W0)
    for x, y in ((8, 3), (13, 8), (8, 13), (3, 8)):
        c.set(x, y, G3)
    c.line(8, 8, 8, 5, G6)
    c.line(8, 8, 11, 9, G6)
    c.set(8, 8, RED1)
    return c


def warning_sign():
    c = Canvas(16, 16)
    for i in range(13):
        c.hline(8 - (i + 1) // 2, 1 + i, (i + 1) // 2 * 2, YEL0)
    c.vline(7, 5, 5, G6)
    c.vline(8, 5, 5, G6)
    c.rect(7, 11, 2, 2, G6)
    c.outline(OUT)
    return c


def extinguisher():
    c = Canvas(16, 32)
    c.rect(4, 8, 8, 22, RED1)
    c.vline(5, 9, 20, RED0)
    c.vline(11, 8, 22, RED2)
    c.rect(5, 4, 6, 4, G5)
    c.rect(10, 3, 4, 2, G6)
    c.line(4, 6, 2, 16, G6)
    c.rect(4, 16, 8, 4, W1)
    c.hline(4, 16, 8, W0)
    c.outline(OUT)
    return c


def poster():
    c = Canvas(24, 32)
    c.rect(0, 0, 24, 32, W0)
    c.rect(2, 2, 20, 24, BLU3)
    import math
    for y in range(4, 25):
        a = math.sin(y / 3.0) * 6
        c.set(round(12 + a), y, TEAL0)
        c.set(round(12 - a), y, PNK0)
        if y % 3 == 0:
            c.hline(round(12 - abs(a)), y, round(abs(a) * 2), with_alpha(W0, 120))
    c.hline(4, 28, 16, G2)
    c.hline(6, 30, 12, G1)
    c.outline(OUT)
    return c


FLOOR_KINDS = ("clean", "speck", "scuff", "drain", "vent", "zone")
WALL_FACE_KINDS = ("plain", "outlet", "vent", "pipe")


def props():
    """name -> (canvas, pivot_px from bottom-left, collider (x, y, w, h) in px from bottom-left or None)."""
    return {
        "BenchComputer": (bench("computer"), (32, 2), (1, 2, 62, 22)),
        "BenchMicroscope": (bench("microscope"), (32, 2), (1, 2, 62, 22)),
        "BenchChem": (bench("chem"), (32, 2), (1, 2, 62, 22)),
        "Shelf": (shelf(), (32, 2), (2, 2, 60, 14)),
        "Plant": (plant(), (16, 4), (9, 4, 14, 8)),
        "Chair": (chair(), (16, 1), (7, 1, 18, 8)),
        "WaterCooler": (water_cooler(), (16, 4), (8, 4, 16, 8)),
        "Bin": (bin_(), (8, 2), (2, 2, 12, 6)),
        "Whiteboard": (whiteboard(), (32, 0), None),
        "WallScreen": (wall_screen(), (24, 0), None),
        "Door": (door(), (32, 0), None),
        "Clock": (clock(), (8, 0), None),
        "WarningSign": (warning_sign(), (8, 0), None),
        "Extinguisher": (extinguisher(), (8, 0), None),
        "Poster": (poster(), (12, 0), None),
        "HazardFloor": (hazard_floor(), (0, 0), None),
        "SoftShadow": (soft_shadow(), (16, 8), None),
    }


def animated_props():
    """name -> (frames, pivot_px, collider)."""
    return {
        "ServerRack": ([server_rack(0), server_rack(1)], (16, 2), (2, 2, 28, 14)),
        "Tank": ([tank(f) for f in range(4)], (16, 2), (1, 2, 30, 14)),
    }
