"""Windy mountain peaks at dusk: tiles, cloud sea, props (32 px per tile, 3/4 top-down)."""
import math
import random

from pixel import Canvas, rgb, with_alpha, mix

T = 32
OUT = rgb("#2b2433")

# rock (plateau top)
G_HI, G_LIGHT, G_BASE = rgb("#bdb3a5"), rgb("#a1978b"), rgb("#887f75")
G_SHADE, G_DARK, G_CRACK = rgb("#6e665e"), rgb("#554f49"), rgb("#3e3935")
# cliff face
C_LIGHT, C_BASE, C_SHADE, C_DARK = rgb("#7e756b"), rgb("#665e56"), rgb("#4f4843"), rgb("#3a3531")
# snow
SN0, SN1, SN2 = rgb("#fff9f4"), rgb("#ecdfe0"), rgb("#c7bcd6")
# grass
GR0, GR1, GR2 = rgb("#b5b56a"), rgb("#8b9450"), rgb("#62703c")
# pine
F_HI, F_LIGHT, F_BASE, F_DARK = rgb("#86a866"), rgb("#5f8a52"), rgb("#416648"), rgb("#2c4836")
BARK_HI, BARK, BARK_SHADE = rgb("#9a6c4a"), rgb("#6e4b33"), rgb("#4a3223")
WOOD_HI, WOOD, WOOD_RING = rgb("#e2b77e"), rgb("#c99a62"), rgb("#a2733f")
# magic
K0, K1, K2 = rgb("#f0ffff"), rgb("#8ff0ff"), rgb("#3cc0e0")
# clouds (sunset sea), darkest -> brightest
CLOUD = [rgb("#6f5f96"), rgb("#8d76ab"), rgb("#b391b8"), rgb("#d9a9b1"), rgb("#f0c2ad"), rgb("#fbdcc0"), rgb("#fff1df")]
SKY = rgb("#a586b5")

BAYER4 = [[0, 8, 2, 10], [12, 4, 14, 6], [3, 11, 1, 9], [15, 7, 13, 5]]


# ---------------------------------------------------------------- noise
def _value_noise(w, h, cell_x, cell_y, rnd):
    gw, gh = max(1, w // cell_x), max(1, h // cell_y)
    grid = [[rnd.random() for _ in range(gw)] for _ in range(gh)]

    def smooth(t):
        return t * t * (3 - 2 * t)

    out = [[0.0] * w for _ in range(h)]
    for y in range(h):
        gy, fy = divmod(y / cell_y, 1)
        gy = int(gy)
        y0, y1 = gy % gh, (gy + 1) % gh
        sy = smooth(fy)
        for x in range(w):
            gx, fx = divmod(x / cell_x, 1)
            gx = int(gx)
            x0, x1 = gx % gw, (gx + 1) % gw
            sx = smooth(fx)
            a = grid[y0][x0] + (grid[y0][x1] - grid[y0][x0]) * sx
            b = grid[y1][x0] + (grid[y1][x1] - grid[y1][x0]) * sx
            out[y][x] = a + (b - a) * sy
    return out


def fbm(w, h, seed, cell_x=64, cell_y=64, octaves=4, persistence=0.5):
    """Seamlessly tiling fractal noise in 0..1 (w, h must be multiples of the base cells)."""
    rnd = random.Random(seed)
    total = [[0.0] * w for _ in range(h)]
    amp, norm = 1.0, 0.0
    for _ in range(octaves):
        layer = _value_noise(w, h, max(2, cell_x), max(2, cell_y), rnd)
        for y in range(h):
            row, lrow = total[y], layer[y]
            for x in range(w):
                row[x] += lrow[x] * amp
        norm += amp
        amp *= persistence
        cell_x //= 2
        cell_y //= 2
    return [[v / norm for v in row] for row in total]


# ---------------------------------------------------------------- ground tiles
def _rock_texture(seed):
    rnd = random.Random(seed)
    c = Canvas(T, T)
    c.rect(0, 0, T, T, G_BASE)
    for _ in range(3):  # soft blotches kept away from the tile border so tiles stay seamless
        cx, cy, rx, ry = rnd.randrange(8, 24), rnd.randrange(8, 24), rnd.randrange(3, 6), rnd.randrange(2, 4)
        col = rnd.choice((G_LIGHT, G_SHADE))
        for y in range(cy - ry, cy + ry + 1):
            for x in range(cx - rx, cx + rx + 1):
                d = ((x - cx) / rx) ** 2 + ((y - cy) / ry) ** 2
                if d < 0.6 or (d < 1.0 and BAYER4[y % 4][x % 4] < 8):
                    c.set(x, y, col)
    for _ in range(rnd.randrange(2, 5)):  # pebbles
        x, y = rnd.randrange(3, 28), rnd.randrange(3, 28)
        c.set(x, y, G_HI)
        c.set(x + 1, y, G_LIGHT)
        c.set(x, y + 1, G_SHADE)
        c.set(x + 1, y + 1, G_SHADE)
    if rnd.random() < 0.6:  # crack
        x, y = rnd.randrange(6, 20), rnd.randrange(6, 24)
        for i in range(rnd.randrange(5, 10)):
            c.set(x, y, G_CRACK)
            c.set(x, y - 1, G_LIGHT)
            x += 1
            y += rnd.choice((-1, 0, 0, 1))
    for _ in range(8):
        x, y = rnd.randrange(2, 30), rnd.randrange(2, 30)
        c.set(x, y, rnd.choice((G_SHADE, G_LIGHT)))
    return c


GROUND_VARIANTS = 4


def ground_tile(variant):
    return _rock_texture(1000 + variant)


def ground_edge(mask):
    """Plateau tile with rims on sides that border the void. mask bits: 1=N, 2=E, 4=S, 8=W neighbour is ground."""
    rnd = random.Random(500 + mask)
    c = _rock_texture(2000 + mask)
    jag = lambda: 1 + rnd.choice((0, 0, 1, 1, 2))
    clear = (0, 0, 0, 0)

    if not mask & 1:  # north: jagged lip catching the light
        for x in range(T):
            d = jag()
            for y in range(d):
                c.replace(x, y, clear)
            c.replace(x, d, OUT)
            c.replace(x, d + 1, G_HI)
            c.replace(x, d + 2, G_LIGHT)
    if not mask & 8:  # west
        for y in range(T):
            d = jag()
            for x in range(d):
                c.replace(x, y, clear)
            if c.get(d, y)[3]:
                c.replace(d, y, OUT)
                c.replace(d + 1, y, G_LIGHT)
    if not mask & 2:  # east (in shadow)
        for y in range(T):
            d = jag()
            for x in range(T - d, T):
                c.replace(x, y, clear)
            if c.get(T - 1 - d, y)[3]:
                c.replace(T - 1 - d, y, OUT)
                c.replace(T - 2 - d, y, G_SHADE)
    if not mask & 4:  # south: straight bright lip; the cliff face continues below
        c.hline(0, T - 2, T, G_LIGHT)
        c.hline(0, T - 1, T, G_HI)
        if not mask & 8:
            c.replace(0, T - 1, clear)
            c.replace(0, T - 2, OUT)
            c.replace(1, T - 1, OUT)
        if not mask & 2:
            c.replace(T - 1, T - 1, clear)
            c.replace(T - 1, T - 2, OUT)
            c.replace(T - 2, T - 1, OUT)
    # round outer corners
    for (open_a, open_b, corner) in ((not mask & 1, not mask & 8, (0, 0)), (not mask & 1, not mask & 2, (T - 1, 0))):
        if open_a and open_b:
            cx, cy = corner
            for y in range(6):
                for x in range(6):
                    if x + y < 5:
                        px = cx + (x if cx == 0 else -x)
                        c.replace(px, cy + y, clear)
                    elif x + y == 5:
                        px = cx + (x if cx == 0 else -x)
                        c.replace(px, cy + y, OUT)
    return c


def cliff_face(kind):
    """South-facing cliff below a plateau edge: 32x64 (upper/lower tiles). kind: mid | left | right | single."""
    rnd = random.Random({"mid": 1, "left": 2, "right": 3, "single": 4}[kind])
    c = Canvas(T, 2 * T)
    c.rect(0, 0, T, 2 * T, C_BASE)
    # irregular vertical fissures of varying length
    for _ in range(9):
        x = rnd.randrange(0, T)
        y = rnd.randrange(0, 40)
        length = rnd.randrange(8, 26)
        for i in range(length):
            c.set(x, y + i, C_DARK)
            c.set(x - 1, y + i, C_LIGHT if i % 5 else C_BASE)
            if rnd.random() < 0.25:
                x += rnd.choice((-1, 1))
    # short broken ledges catching the light
    for _ in range(7):
        x0, y0, length = rnd.randrange(-4, T), rnd.randrange(6, 52), rnd.randrange(4, 11)
        for i in range(length):
            c.set(x0 + i, y0, C_LIGHT)
            c.set(x0 + i, y0 + 1, C_SHADE)
            if rnd.random() < 0.2:
                y0 += rnd.choice((-1, 1))
    # mottling
    for _ in range(70):
        x, y = rnd.randrange(T), rnd.randrange(3, 2 * T)
        c.set(x, y, rnd.choice((C_SHADE, C_SHADE, C_LIGHT, C_DARK)))
    # shadow under the lip
    c.hline(0, 0, T, C_DARK)
    c.hline(0, 1, T, C_SHADE)
    # tapered ends so outer corners recede
    taper_left, taper_right = kind in ("left", "single"), kind in ("right", "single")
    for y in range(2 * T):
        t = min(10, y // 5)
        if taper_left:
            for xx in range(t):
                c.replace(xx, y, (0, 0, 0, 0))
            c.replace(t, y, OUT)
        if taper_right:
            for xx in range(T - t, T):
                c.replace(xx, y, (0, 0, 0, 0))
            c.replace(T - 1 - t, y, OUT)
    # dissolve into the clouds
    for y in range(34, 2 * T):
        a = 1 - (y - 34) / (2 * T - 34)
        for xx in range(T):
            p = c.get(xx, y)
            if p[3]:
                threshold = BAYER4[y % 4][xx % 4] / 16
                if a < threshold * 0.9:
                    c.replace(xx, y, (0, 0, 0, 0))
                else:
                    c.replace(xx, y, with_alpha(mix(p, CLOUD[2], 1 - a), p[3]))
    return c


CLIFF_KINDS = ("mid", "left", "right", "single")


# ---------------------------------------------------------------- clouds
SEA = [rgb("#8f7db3"), rgb("#ae93bf"), rgb("#d1aabd"), rgb("#ecc2b6"), rgb("#f9dcc4"), rgb("#fff0de")]


def cloud_sea():
    """Opaque, seamlessly tiling sea of soft sunset clouds (512x256)."""
    w, h = 512, 256
    v = fbm(w, h, 77, cell_x=256, cell_y=128, octaves=5, persistence=0.45)
    c = Canvas(w, h)
    for y in range(h):
        for x in range(w):
            val = v[y][x]
            light = val - v[(y + 4) % h][(x + 4) % w]   # puffs lit from the upper-left
            f = (val - 0.3) / 0.42 * (len(SEA) - 1) + light * 9
            f += (BAYER4[y % 4][x % 4] / 16 - 0.5) * 0.55
            c.replace(x, y, SEA[max(0, min(len(SEA) - 1, int(f)))])
    return c


def cloud_wisps():
    """Sparse, wind-stretched translucent streaks drawn above the peaks (256x128)."""
    w, h = 256, 128
    v = fbm(w, h, 91, cell_x=128, cell_y=16, octaves=3, persistence=0.5)
    c = Canvas(w, h)
    for y in range(h):
        for x in range(w):
            val = v[y][x]
            if val > 0.66:
                a = min(95, int((val - 0.66) * 700))
                if a > 45 or BAYER4[y % 4][x % 4] < 6:
                    c.replace(x, y, with_alpha(SEA[5], max(25, a)))
    return c


def distant_peak(variant):
    rnd = random.Random(300 + variant)
    w, h = 128, 96
    c = Canvas(w, h)
    apex = (rnd.randrange(50, 78), rnd.randrange(6, 16))
    ridge = []
    for x in range(w):
        dist = abs(x - apex[0])
        y = apex[1] + dist * (1.15 + 0.25 * math.sin(x * 0.15 + variant)) + rnd.choice((0, 0, 1))
        ridge.append(min(h, int(y)))
    lit, shade = rgb("#c7a3c3"), rgb("#8f78aa")
    snow_lit, snow_shade = rgb("#fff1e8"), rgb("#cbbde2")
    for x in range(w):
        for y in range(ridge[x], h):
            on_lit_side = x < apex[0] + (y - apex[1]) * 0.15
            depth = y - ridge[x]
            snowy = y < apex[1] + 26 + (x * 7 % 9) and depth < 18
            col = (snow_lit if on_lit_side else snow_shade) if snowy else (lit if on_lit_side else shade)
            fade = (y - (h - 40)) / 40
            if fade > 0:
                if fade > 0.95 or BAYER4[y % 4][x % 4] / 16 < fade:
                    continue
                col = mix(col, CLOUD[2], fade * 0.6)
            c.replace(x, y, col)
    return c


# ---------------------------------------------------------------- props
def pine_tree():
    w, h = 48, 84
    c = Canvas(w, h)
    # trunk, bending east with height
    for y in range(40, 80):
        t = (80 - y) / 40
        x0 = round(22 + t * 2)
        width = 5 if y > 70 else 4
        c.hline(x0, y, width, BARK)
        c.set(x0, y, BARK_HI)
        c.set(x0 + width - 1, y, BARK_SHADE)
    c.hline(19, 79, 11, BARK_SHADE)  # root flare
    c.hline(20, 78, 9, BARK)
    tiers = [(8, 22, 7), (18, 34, 11), (29, 47, 14), (41, 60, 17), (53, 72, 19)]
    for i, (top, bottom, half) in enumerate(tiers):
        cx = 24 + (len(tiers) - i) * 1.2
        for y in range(top, bottom + 1):
            t = (y - top) / max(1, bottom - top)
            hw = max(1, round(half * t))
            jag = 2 if (y % 3 == 0 and t > 0.6) else 0
            x0, x1 = round(cx - hw), round(cx + hw + 3 * t)
            for x in range(x0 + jag, x1 + 1):
                rel = (x - x0) / max(1, x1 - x0)
                col = F_LIGHT if rel < 0.3 else F_BASE if rel < 0.75 else F_DARK
                if y >= bottom - 1:
                    col = F_DARK
                c.set(x, y, col)
        # snow resting on the upper-left of each tier
        for y in range(top + 1, bottom - 2):
            t = (y - top) / max(1, bottom - top)
            hw = max(1, round(half * t))
            x0 = round(cx - hw)
            c.set(x0, y, SN0)
            if t < 0.7:
                c.set(x0 + 1, y, SN1)
        c.set(round(cx), top, SN0)
        for x in range(round(cx - half * 0.6), round(cx + half * 0.2)):
            if x % 3 == 0:
                c.set(x, bottom - 1, F_HI)
    c.outline(OUT)
    return c


def stump():
    c = Canvas(32, 24)
    c.ellipse(16, 18, 13, 5, BARK_SHADE)
    for x0, x1 in ((4, 9), (23, 28)):
        c.hline(x0, 20, x1 - x0, BARK)
    c.rect(8, 9, 16, 11, BARK)
    c.vline(8, 9, 11, BARK_HI)
    c.vline(23, 9, 11, BARK_SHADE)
    c.ellipse(16, 9, 8.5, 3.8, WOOD)
    c.ellipse(16, 9, 6, 2.6, WOOD_RING)
    c.ellipse(16, 9, 4.5, 1.8, WOOD)
    c.ellipse(16, 9, 2, 1, WOOD_RING)
    c.hline(10, 7, 6, WOOD_HI)
    c.outline(OUT)
    return c


def chopping_block():
    c = Canvas(32, 24)
    c.rect(5, 10, 22, 10, BARK)
    c.vline(5, 10, 10, BARK_HI)
    c.vline(26, 10, 10, BARK_SHADE)
    c.ellipse(16, 10, 11.5, 4.5, WOOD)
    c.ellipse(16, 10, 8.5, 3, WOOD_RING)
    c.ellipse(16, 10, 6, 2, WOOD)
    c.ellipse(16, 10, 2.5, 1, WOOD_RING)
    c.line(13, 9, 19, 11, rgb("#8a5f38"))  # old cut mark
    c.hline(6, 20, 20, BARK_SHADE)
    c.outline(OUT)
    return c


def axe_in_block():
    """Bronze axe wedged in the chopping block (drawn over it as a separate ground item)."""
    c = Canvas(24, 24)
    handle, handle_hi = rgb("#7a5232"), rgb("#a3714a")
    c.line(5, 4, 13, 15, handle)
    c.line(6, 4, 14, 15, handle_hi)
    hi, base, shade = rgb("#f0c08a"), rgb("#c8803f"), rgb("#8a4f22")
    for i, wdt in enumerate((3, 5, 6, 6, 5)):
        c.hline(12, 14 + i, wdt, base)
        c.set(12 + wdt - 1, 14 + i, hi)
    c.hline(12, 18, 5, shade)
    c.outline(OUT)
    return c


def boulder(size, snowy=False, seed=0):
    rnd = random.Random(seed)
    w, h = (40, 32) if size == "big" else (26, 20)
    c = Canvas(w, h)
    cx, cy, rx, ry = w / 2, h * 0.55, w / 2 - 2, h * 0.42
    for y in range(h):
        for x in range(w):
            dx, dy = (x + 0.5 - cx) / rx, (y + 0.5 - cy) / ry
            d = dx * dx + dy * dy + 0.08 * math.sin(x * 0.9 + seed) * math.cos(y * 0.7)
            if d <= 1:
                lightness = -dx * 0.5 - dy * 0.7
                col = G_HI if lightness > 0.55 else G_LIGHT if lightness > 0.1 else G_BASE if lightness > -0.35 else G_SHADE
                if dy > 0.55:
                    col = G_DARK
                c.set(x, y, col)
    for _ in range(3):
        x, y = rnd.randrange(int(w * 0.3), int(w * 0.7)), rnd.randrange(int(h * 0.3), int(h * 0.7))
        c.set(x, y, G_CRACK)
        c.set(x + 1, y + 1, G_CRACK)
    if snowy:
        for y in range(h):
            for x in range(w):
                if c.get(x, y)[3] and (y + 0.5 - cy) / ry < -0.35 + 0.15 * math.sin(x * 0.7):
                    c.replace(x, y, SN0 if (x + 0.5 - cx) < 0 else SN1)
    c.outline(OUT)
    return c


def cairn():
    c = Canvas(22, 30)
    stones = [(11, 25, 9, 3.5), (10, 19, 7, 3), (12, 14, 6, 2.6), (11, 9, 4.5, 2.2), (11, 5, 3, 1.8)]
    for x, y, rx, ry in stones:
        c.ellipse(x, y, rx, ry, G_SHADE)
        c.ellipse(x - 0.5, y - 0.6, rx - 1, ry - 1, G_LIGHT)
        c.hline(round(x - rx * 0.5), round(y - ry + 0.5), max(1, round(rx * 0.6)), G_HI)
    c.outline(OUT)
    return c


def standing_stone(variant):
    rnd = random.Random(40 + variant)
    c = Canvas(16, 38)
    for y in range(2, 36):
        t = y / 36
        x0 = 3 + (1 if y < 6 else 0) + (rnd.random() < 0.2)
        x1 = 12 - (1 if y < 4 else 0) + (variant if y < 10 else 0)
        c.hline(x0, y, x1 - x0 + 1, G_BASE)
        c.set(x0, y, G_LIGHT)
        c.set(x0 + 1, y, G_LIGHT)
        c.set(x1, y, G_SHADE)
    c.hline(4, 2, 7, G_HI)
    for y in (10, 14, 18, 22):
        c.set(7, y, K2)
        c.set(8, y + 1, K1)
    c.hline(2, 36, 12, G_DARK)
    c.outline(OUT)
    return c


def ritual_circle():
    w, h = 96, 56
    c = Canvas(w, h)
    groove, lip = with_alpha(G_DARK, 200), with_alpha(G_LIGHT, 160)
    for ring, (rx, ry) in enumerate(((44, 25), (36, 20))):
        for i in range(720):
            a = i / 720 * math.tau
            x, y = round(w / 2 + math.cos(a) * rx), round(h / 2 + math.sin(a) * ry)
            c.set(x, y, groove)
            c.set(x, y - 1, lip)
    for i in range(8):
        a = i / 8 * math.tau + 0.2
        x, y = round(w / 2 + math.cos(a) * 40), round(h / 2 + math.sin(a) * 22.5)
        for dx, dy in ((0, -1), (0, 0), (0, 1), (-1, 0) if i % 2 else (1, 0), (1, -1) if i % 3 else (-1, 1)):
            c.set(x + dx, y + dy, K1)
        c.set(x, y - 2, with_alpha(K0, 160))
    c.soft_ellipse(w / 2, h / 2, 14, 8, rgb("#2b2433", 90))
    return c


PENNANT_COLORS = [rgb("#c94a4a"), rgb("#e8d9b0"), rgb("#4a74c9"), rgb("#d9a93e"), rgb("#4f9a5a"), rgb("#c94a4a"), rgb("#e8d9b0")]


def prayer_flags(frame):
    w, h = 96, 44
    c = Canvas(w, h)
    pole, pole_hi = rgb("#6e4b33"), rgb("#9a6c4a")
    for px in (3, 90):
        c.rect(px, 4, 3, 38, pole)
        c.vline(px, 4, 38, pole_hi)
        c.rect(px - 1, 3, 5, 2, rgb("#4a3223"))
    rope = rgb("#5a4535")
    pts = []
    for x in range(6, 90):
        t = (x - 6) / 84
        y = round(7 + math.sin(t * math.pi) * 7)
        pts.append((x, y))
        c.set(x, y, rope)
    for i in range(7):
        x, y = pts[8 + i * 11]
        col = PENNANT_COLORS[i]
        flutter = (frame + i) % 3
        # triangular pennant blown east by the wind
        for row in range(7):
            length = 7 - row
            dx = flutter if row > 2 else 0
            for k in range(length):
                c.set(x + k + dx * (k > 2), y + 1 + row, mix(col, OUT, 0.25) if k == length - 1 else col)
    c.outline(OUT)
    return c


def rune_slab(glowing):
    c = Canvas(32, 22)
    c.ellipse(16, 12, 14.5, 8.5, G_SHADE)
    c.ellipse(15.5, 11, 13.5, 7.5, G_LIGHT)
    c.ellipse(15, 10.5, 11, 5.5, G_BASE)
    col = K1 if glowing else G_DARK
    # carved feather rune
    c.line(10, 14, 21, 7, col)
    for i in range(4):
        x, y = 12 + i * 2, 13 - i * 2 + (1 if i % 2 else 0)
        c.set(x, y - 2, col)
        c.set(x + 1, y + 2, col)
    if glowing:
        c.set(21, 6, K0)
        c.set(9, 15, K0)
    c.outline(OUT)
    return c


def snow_drift(variant):
    sizes = ((48, 20), (36, 16), (56, 24))
    w, h = sizes[variant]
    rnd = random.Random(60 + variant)
    c = Canvas(w, h)
    blobs = [(w * 0.5, h * 0.55, w * 0.45, h * 0.4)] + [
        (rnd.uniform(w * 0.2, w * 0.8), rnd.uniform(h * 0.35, h * 0.65), rnd.uniform(w * 0.15, w * 0.3), rnd.uniform(h * 0.2, h * 0.35))
        for _ in range(3)]
    for y in range(h):
        for x in range(w):
            inside = max(1 - (((x - bx) / rx) ** 2 + ((y - by) / ry) ** 2) for bx, by, rx, ry in blobs)
            if inside > 0.25 or (inside > 0 and BAYER4[y % 4][x % 4] / 16 < inside * 3):
                top = y < h * 0.45
                c.set(x, y, SN0 if top and inside > 0.4 else SN1 if inside > 0.3 else SN2)
    return c


def grass_tuft(frame, variant=0):
    c = Canvas(12, 12)
    blades = [(2, 6, -1), (4, 9, 0), (6, 10, 1), (8, 8, 1), (10, 5, 2)]
    sway = (0, 1, 2, 1)[frame]
    for i, (x, height, lean) in enumerate(blades):
        col = (GR1, GR0, GR1, GR2, GR1)[(i + variant) % 5]
        for k in range(height):
            t = k / height
            bx = x + round((lean + sway) * t * t * 1.4)
            c.set(bx, 11 - k, col if k > 1 else GR2)
    return c


def dead_bush():
    c = Canvas(22, 18)
    twig, twig_hi = rgb("#6e5340"), rgb("#93735a")
    for (x0, y0, x1, y1) in ((11, 17, 11, 9), (11, 12, 5, 5), (11, 11, 17, 4), (11, 14, 18, 10), (11, 13, 3, 11), (7, 8, 6, 3), (15, 7, 18, 2)):
        c.line(x0, y0, x1, y1, twig)
    for (x, y) in ((5, 5), (17, 4), (18, 10), (3, 11)):
        c.set(x, y, twig_hi)
    c.outline(OUT)
    return c


def campfire(frame):
    rnd = random.Random(frame * 13)
    c = Canvas(32, 32)
    for i in range(8):  # stone ring
        a = i / 8 * math.tau
        x, y = 16 + math.cos(a) * 11, 23 + math.sin(a) * 5
        c.ellipse(x, y, 3, 2.2, G_SHADE)
        c.ellipse(x - 0.4, y - 0.6, 2, 1.4, G_LIGHT)
    for (x0, y0, x1, y1) in ((8, 26, 24, 20), (8, 20, 24, 26)):  # crossed logs
        c.line(x0, y0, x1, y1, BARK)
        c.line(x0, y0 - 1, x1, y1 - 1, BARK_HI)
    c.outline(OUT)
    flame_cols = [rgb("#fff3b0"), rgb("#ffd25a"), rgb("#ff9a2e"), rgb("#e0531f"), rgb("#a8321c")]
    tongues = [(16, 4), (12, 9), (20, 8), (14, 7), (18, 6)]
    for i, (fx, height) in enumerate(tongues):
        h = height + rnd.randrange(-2, 3) + (frame % 2)
        sway = rnd.choice((-1, 0, 1))
        for k in range(h * 2):
            t = k / (h * 2)
            y = 22 - k // 1
            if y < 22 - h * 2 + 2:
                break
            wdt = max(1, round((1 - t) * (4 - i % 2)))
            x0 = fx + round(sway * t * 2) - wdt // 2
            col = flame_cols[min(4, int(t * 3) + (1 if i else 0))] if wdt > 1 else flame_cols[2]
            c.hline(x0, y, wdt, col)
    c.set(16, 22 - rnd.randrange(14, 18), rgb("#ffd25a"))
    return c


def ashes():
    c = Canvas(32, 32)
    for i in range(8):
        a = i / 8 * math.tau
        x, y = 16 + math.cos(a) * 11, 23 + math.sin(a) * 5
        c.ellipse(x, y, 3, 2.2, G_SHADE)
        c.ellipse(x - 0.4, y - 0.6, 2, 1.4, G_LIGHT)
    c.ellipse(16, 23, 7, 3, rgb("#5e5753"))
    c.ellipse(16, 22.5, 5, 2, rgb("#7c7470"))
    for (x, y) in ((13, 22), (18, 23), (16, 21)):
        c.set(x, y, rgb("#2b2433"))
    c.set(15, 23, rgb("#e0531f"))
    c.outline(OUT)
    return c
