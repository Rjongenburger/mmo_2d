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
