"""Tiny dependency-free pixel canvas + PNG writer used by the art generator."""
import struct
import zlib

CLEAR = (0, 0, 0, 0)


def rgb(hex_str, a=255):
    s = hex_str.lstrip("#")
    return (int(s[0:2], 16), int(s[2:4], 16), int(s[4:6], 16), a)


def with_alpha(c, a):
    return (c[0], c[1], c[2], a)


def mix(c1, c2, t):
    return tuple(round(c1[i] + (c2[i] - c1[i]) * t) for i in range(4))


def ramp(colors, t):
    """Smoothly blends along a palette; t runs from 0 (first colour) to len(colors) - 1 (last)."""
    t = max(0.0, min(len(colors) - 1.0, t))
    i = min(int(t), len(colors) - 2)
    return mix(colors[i], colors[i + 1], t - i)


class Canvas:
    def __init__(self, w, h):
        self.w, self.h = w, h
        self.px = [[CLEAR] * w for _ in range(h)]

    # ---- pixel access -------------------------------------------------
    def get(self, x, y):
        if 0 <= x < self.w and 0 <= y < self.h:
            return self.px[y][x]
        return CLEAR

    def set(self, x, y, c):
        """Alpha-composites c over the existing pixel."""
        if c is None or not (0 <= x < self.w and 0 <= y < self.h):
            return
        if c[3] == 255:
            self.px[y][x] = c
            return
        if c[3] == 0:
            return
        dst = self.px[y][x]
        sa, da = c[3] / 255, dst[3] / 255
        oa = sa + da * (1 - sa)
        if oa == 0:
            self.px[y][x] = CLEAR
            return
        out = [round((c[i] * sa + dst[i] * da * (1 - sa)) / oa) for i in range(3)]
        self.px[y][x] = (out[0], out[1], out[2], round(oa * 255))

    def replace(self, x, y, c):
        if 0 <= x < self.w and 0 <= y < self.h:
            self.px[y][x] = c

    # ---- primitives ---------------------------------------------------
    def rect(self, x, y, w, h, c):
        for yy in range(y, y + h):
            for xx in range(x, x + w):
                self.set(xx, yy, c)

    def hline(self, x, y, w, c):
        self.rect(x, y, w, 1, c)

    def vline(self, x, y, h, c):
        self.rect(x, y, 1, h, c)

    def frame(self, x, y, w, h, c):
        self.hline(x, y, w, c)
        self.hline(x, y + h - 1, w, c)
        self.vline(x, y, h, c)
        self.vline(x + w - 1, y, h, c)

    def ellipse(self, cx, cy, rx, ry, c):
        for yy in range(int(cy - ry) - 1, int(cy + ry) + 2):
            for xx in range(int(cx - rx) - 1, int(cx + rx) + 2):
                dx, dy = (xx + 0.5 - cx) / rx, (yy + 0.5 - cy) / ry
                if dx * dx + dy * dy <= 1.0:
                    self.set(xx, yy, c)

    def soft_ellipse(self, cx, cy, rx, ry, c, falloff=0.45):
        """Ellipse whose alpha fades towards the edge (for shadows / glows)."""
        for yy in range(int(cy - ry) - 1, int(cy + ry) + 2):
            for xx in range(int(cx - rx) - 1, int(cx + rx) + 2):
                dx, dy = (xx + 0.5 - cx) / rx, (yy + 0.5 - cy) / ry
                d = (dx * dx + dy * dy) ** 0.5
                if d <= 1.0:
                    t = 1.0 if d < 1 - falloff else (1.0 - d) / falloff
                    self.set(xx, yy, with_alpha(c, round(c[3] * t)))

    def line(self, x0, y0, x1, y1, c):
        dx, dy = abs(x1 - x0), -abs(y1 - y0)
        sx, sy = (1 if x0 < x1 else -1), (1 if y0 < y1 else -1)
        err = dx + dy
        while True:
            self.set(x0, y0, c)
            if x0 == x1 and y0 == y1:
                break
            e2 = 2 * err
            if e2 >= dy:
                err += dy
                x0 += sx
            if e2 <= dx:
                err += dx
                y0 += sy

    def ascii(self, rows, palette, ox=0, oy=0, flip=False):
        """Draws an ASCII sprite; '.' and ' ' are transparent."""
        width = max(len(r) for r in rows)
        for y, row in enumerate(rows):
            for x, ch in enumerate(row):
                if ch in ". ":
                    continue
                tx = ox + (width - 1 - x if flip else x)
                self.set(tx, oy + y, palette[ch])

    def blit(self, src, ox, oy, flip=False):
        for y in range(src.h):
            for x in range(src.w):
                c = src.px[y][src.w - 1 - x if flip else x]
                if c[3]:
                    self.set(ox + x, oy + y, c)

    def outline(self, c, diagonal=False):
        """Adds a 1px outline around opaque pixels (in place)."""
        opaque = [[self.px[y][x][3] > 0 for x in range(self.w)] for y in range(self.h)]
        offsets = [(1, 0), (-1, 0), (0, 1), (0, -1)]
        if diagonal:
            offsets += [(1, 1), (-1, -1), (1, -1), (-1, 1)]
        for y in range(self.h):
            for x in range(self.w):
                if opaque[y][x]:
                    continue
                for dx, dy in offsets:
                    nx, ny = x + dx, y + dy
                    if 0 <= nx < self.w and 0 <= ny < self.h and opaque[ny][nx]:
                        self.px[y][x] = c
                        break

    def flipped(self):
        out = Canvas(self.w, self.h)
        out.blit(self, 0, 0, flip=True)
        return out

    def crop(self, x, y, w, h):
        out = Canvas(w, h)
        for yy in range(h):
            for xx in range(w):
                out.px[yy][xx] = self.get(x + xx, y + yy)
        return out

    def scaled(self, k):
        out = Canvas(self.w * k, self.h * k)
        for y in range(out.h):
            row = self.px[y // k]
            out.px[y] = [row[x // k] for x in range(out.w)]
        return out

    # ---- output -------------------------------------------------------
    def save(self, path):
        raw = b"".join(b"\x00" + bytes(v for p in row for v in p) for row in self.px)

        def chunk(tag, data):
            body = tag + data
            return struct.pack(">I", len(data)) + body + struct.pack(">I", zlib.crc32(body) & 0xFFFFFFFF)

        png = b"\x89PNG\r\n\x1a\n"
        png += chunk(b"IHDR", struct.pack(">IIBBBBB", self.w, self.h, 8, 6, 0, 0, 0))
        png += chunk(b"IDAT", zlib.compress(raw, 9))
        png += chunk(b"IEND", b"")
        with open(path, "wb") as f:
            f.write(png)


def sheet(frames, cols):
    """Packs equally sized canvases into a grid (row-major)."""
    fw, fh = frames[0].w, frames[0].h
    rows = (len(frames) + cols - 1) // cols
    out = Canvas(fw * cols, fh * rows)
    for i, f in enumerate(frames):
        out.blit(f, (i % cols) * fw, (i // cols) * fh)
    return out


# ---- smoothing --------------------------------------------------------
SMOOTH_SCALE = 4


def _scale2x(src):
    """EPX / Scale2x: doubles a canvas, rounding off stair-stepped diagonals without inventing colours."""
    w, h, px = src.w, src.h, src.px
    out = Canvas(w * 2, h * 2)
    for y in range(h):
        row, up, down = px[y], px[max(y - 1, 0)], px[min(y + 1, h - 1)]
        top, bottom = out.px[2 * y], out.px[2 * y + 1]
        for x in range(w):
            p, a, d = row[x], up[x], down[x]
            c, b = row[max(x - 1, 0)], row[min(x + 1, w - 1)]
            if c != b and a != d:
                top[2 * x] = a if c == a else p
                top[2 * x + 1] = b if a == b else p
                bottom[2 * x] = c if d == c else p
                bottom[2 * x + 1] = d if b == d else p
            else:
                top[2 * x] = top[2 * x + 1] = bottom[2 * x] = bottom[2 * x + 1] = p
    return out


def _halved(src):
    """2x2 box downsample in premultiplied alpha, so edges get anti-aliased instead of haloed."""
    out = Canvas(src.w // 2, src.h // 2)
    for y in range(out.h):
        r0, r1, orow = src.px[2 * y], src.px[2 * y + 1], out.px[y]
        for x in range(out.w):
            q = (r0[2 * x], r0[2 * x + 1], r1[2 * x], r1[2 * x + 1])
            a = q[0][3] + q[1][3] + q[2][3] + q[3][3]
            if a:
                orow[x] = tuple((sum(c[i] * c[3] for c in q) + a // 2) // a for i in range(3)) + ((a + 2) // 4,)
    return out


def _luma(c):
    return 0.299 * c[0] + 0.587 * c[1] + 0.114 * c[2]


def _soften_outlines(canvas, threshold=70, amount=0.6):
    """Swaps the near-black silhouette outline for a darker shade of the colour just inside it, so shapes
    read as shaded edges rather than cartoon ink lines. Inner lines (between parts) are kept."""
    px, w, h = canvas.px, canvas.w, canvas.h
    get = lambda x, y: px[y][x] if 0 <= x < w and 0 <= y < h else CLEAR
    out = [row[:] for row in px]
    for y in range(h):
        for x in range(w):
            p = px[y][x]
            if not p[3] or _luma(p) > threshold:
                continue
            if all(get(x + dx, y + dy)[3] for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1))):
                continue
            # the nearest clearly lighter, solid colours within two pixels are what the outline is wrapping
            for r in (1, 2):
                inner = [n for dy in range(-r, r + 1) for dx in range(-r, r + 1)
                         for n in (get(x + dx, y + dy),) if n[3] == 255 and _luma(n) > _luma(p) + 30]
                if inner:
                    avg = tuple(sum(n[i] for n in inner) // len(inner) for i in range(3)) + (p[3],)
                    out[y][x] = mix(p, avg, amount)
                    break
    canvas.px = out


def _box_blur(grid, w, h, r):
    """Separable box blur of a float grid with clamped edges."""
    for horizontal in (True, False):
        n = w if horizontal else h
        out = [[0.0] * w for _ in range(h)]
        for j in range(h if horizontal else w):
            line = grid[j] if horizontal else [grid[i][j] for i in range(h)]
            acc = line[0] * (r + 1) + sum(line[min(i, n - 1)] for i in range(1, r + 1))
            for i in range(n):
                v = acc / (2 * r + 1)
                if horizontal:
                    out[j][i] = v
                else:
                    out[i][j] = v
                acc += line[min(i + r + 1, n - 1)] - line[max(i - r, 0)]
        grid = out
    return grid


def _form_shade(canvas, radius, strength=0.3):
    """Soft volume shading: lightens edges facing the upper-left light and darkens those facing away,
    using the blurred silhouette as a height field. Fully opaque areas (tiles) are left untouched."""
    w, h = canvas.w, canvas.h
    height = [[p[3] / 255 for p in row] for row in canvas.px]
    for _ in range(2):
        height = _box_blur(height, w, h, radius)
    lx, ly = -0.6, -0.8   # towards the light, in image space (y down)
    for y in range(h):
        row, hy0, hy1 = canvas.px[y], height[max(y - 1, 0)], height[min(y + 1, h - 1)]
        hrow = height[y]
        for x in range(w):
            p = row[x]
            if not p[3]:
                continue
            gx = hrow[min(x + 1, w - 1)] - hrow[max(x - 1, 0)]
            gy = hy1[x] - hy0[x]
            # the height falls off towards the edge, so -gradient is the outward-facing normal
            facing = -(gx * lx + gy * ly) * radius * 1.6
            k = 1 + max(-1.0, min(1.0, facing)) * strength
            row[x] = (min(255, round(p[0] * k)), min(255, round(p[1] * k)), min(255, round(p[2] * k)), p[3])


def _value_noise(w, h, cell, seed):
    """Smooth value noise in [-1, 1] that wraps around the canvas size, so tiles stay seamless."""
    import random
    rnd = random.Random(seed)
    gw, gh = max(1, round(w / cell)), max(1, round(h / cell))
    grid = [[rnd.uniform(-1, 1) for _ in range(gw)] for _ in range(gh)]
    s = lambda t: t * t * (3 - 2 * t)
    out = []
    for y in range(h):
        fy = y / h * gh
        y0 = int(fy) % gh
        y1, ty = (y0 + 1) % gh, s(fy - int(fy))
        row = []
        for x in range(w):
            fx = x / w * gw
            x0 = int(fx) % gw
            x1, tx = (x0 + 1) % gw, s(fx - int(fx))
            top = grid[y0][x0] + (grid[y0][x1] - grid[y0][x0]) * tx
            bottom = grid[y1][x0] + (grid[y1][x1] - grid[y1][x0]) * tx
            row.append(top + (bottom - top) * ty)
        out.append(row)
    return out


def _grain(canvas, strength=0.06):
    """Subtle surface variation so large fills read as material (stone, wood, cloth) instead of flat paint."""
    w, h = canvas.w, canvas.h
    coarse = _value_noise(w, h, SMOOTH_SCALE * 3, 1)
    fine = _value_noise(w, h, SMOOTH_SCALE, 2)
    for y in range(h):
        row = canvas.px[y]
        for x in range(w):
            p = row[x]
            if p[3]:
                k = 1 + (coarse[y][x] * 0.65 + fine[y][x] * 0.35) * strength
                row[x] = (min(255, round(p[0] * k)), min(255, round(p[1] * k)), min(255, round(p[2] * k)), p[3])


def smoothed(canvas, soften=False, shade=False, grain=False):
    """The art is drawn at pixel scale; this renders it SMOOTH_SCALE times larger with clean, anti-aliased
    edges (Scale2x three times, then halved) so it can be shown with bilinear filtering instead of as pixels.
    The options push world art towards a more natural look: soften swaps silhouette ink outlines for shaded
    edges, shade adds soft volume lit from the upper-left and grain adds surface texture."""
    out = Canvas(canvas.w, canvas.h)
    out.px = [[p if p[3] else CLEAR for p in row] for row in canvas.px]
    if soften:
        _soften_outlines(out)
    for _ in range(3):
        out = _scale2x(out)
    out = _halved(out)
    if shade:
        _form_shade(out, radius=SMOOTH_SCALE * 2)
    if grain:
        _grain(out)
    return out


def extruded(canvas, pad):
    """Surrounds a canvas with copies of its edge pixels, so bilinear sampling at a sheet cell's border
    never picks up the neighbouring cell."""
    out = Canvas(canvas.w + 2 * pad, canvas.h + 2 * pad)
    for y in range(out.h):
        row = canvas.px[min(max(y - pad, 0), canvas.h - 1)]
        out.px[y] = [row[min(max(x - pad, 0), canvas.w - 1)] for x in range(out.w)]
    return out
