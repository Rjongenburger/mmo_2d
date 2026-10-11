"""Wind crow: a small hostile bird. 24x20 frames facing right: flap0..flap3, dive."""
import math

from pixel import Canvas, rgb

FRAME_W, FRAME_H = 24, 20
PIVOT = (12, 10)
COLUMNS = ("flap0", "flap1", "flap2", "flap3", "dive")

OUT = rgb("#1c1624")
BODY, BODY_HI, BODY_SH = rgb("#3a3150"), rgb("#5b4f7a"), rgb("#28213a")
WING, WING_HI = rgb("#30283f"), rgb("#4c4166")
BEAK, BEAK_SH = rgb("#f0b93a"), rgb("#b8801c")
EYE = rgb("#ff5a4a")


def _fill_polygon(c, points, color):
    ys = [p[1] for p in points]
    for y in range(int(min(ys)), int(max(ys)) + 1):
        xs = []
        for (x0, y0), (x1, y1) in zip(points, points[1:] + points[:1]):
            if (y0 <= y < y1) or (y1 <= y < y0):
                xs.append(x0 + (y - y0) * (x1 - x0) / (y1 - y0))
        xs.sort()
        for a, b in zip(xs[0::2], xs[1::2]):
            for x in range(round(a), round(b) + 1):
                c.set(x, y, color)


def _wing(c, shoulder, tips, color, highlight):
    """Broad wing: a filled fan from the shoulder through the feather tips, with a lit leading edge."""
    back = (shoulder[0] - 3, shoulder[1] + 1)
    _fill_polygon(c, [shoulder] + tips + [back], color)
    lead = tips[-1]
    c.line(shoulder[0], shoulder[1], lead[0], lead[1], highlight)
    for tip in tips[:-1]:  # separate primary feathers
        c.set(tip[0], tip[1], highlight)


def frame(index):
    c = Canvas(FRAME_W, FRAME_H)
    dive = index == 4
    tilt = 2 if dive else 0
    # tail fan
    for k, (x, y) in enumerate(((3, 10 + tilt), (3, 12 + tilt), (4, 14 + tilt))):
        c.line(x, y, 8, 11 + tilt // 2, BODY_SH if k else BODY)
    # body and head
    c.ellipse(11.5, 11.5 + tilt * 0.5, 5.2, 3.4, BODY)
    c.ellipse(11, 10.5 + tilt * 0.5, 3.5, 1.6, BODY_HI)
    c.hline(8, 14 + tilt // 2, 7, BODY_SH)
    c.ellipse(16.5, 9.5 + tilt, 2.8, 2.6, BODY)
    c.set(16, 8 + tilt, BODY_HI)
    c.set(17, 9 + tilt, EYE)
    # beak
    c.hline(19, 9 + tilt, 3, BEAK)
    c.hline(19, 10 + tilt, 2, BEAK_SH)
    c.set(22, 9 + tilt, BEAK_SH)
    # wings
    shoulder = (11, 9 + tilt // 2)
    if dive:
        _wing(c, shoulder, [(2, 9), (4, 8), (8, 8), (12, 9)], WING, WING_HI)
    else:
        spread = {0: [(3, 2), (6, 0), (10, 1), (13, 4)], 1: [(1, 8), (3, 6), (7, 5), (12, 7)],
                  2: [(4, 17), (7, 18), (10, 17), (13, 13)], 3: [(1, 10), (4, 8), (8, 7), (12, 8)]}[index]
        _wing(c, shoulder, spread, WING, WING_HI)
    # feet tucked under the body
    c.set(11, 15 + tilt // 2, BEAK_SH)
    c.set(13, 15 + tilt // 2, BEAK_SH)
    c.outline(OUT)
    return c


def build_frames():
    return [frame(i) for i in range(len(COLUMNS))]


def frame_names(prefix="Crow"):
    return [f"{prefix}_{col}" for col in COLUMNS]
