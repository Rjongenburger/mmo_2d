"""OSRS-flavoured HUD pieces, item icons and skill icons."""
import random

from pixel import Canvas, rgb, with_alpha, mix

UI_OUT = rgb("#0e0b08")
ST_DARK, ST, ST_MID, ST_HI = rgb("#2b241c"), rgb("#3e352a"), rgb("#4d4334"), rgb("#6b5d47")
PARCH, PARCH_HI, PARCH_SH, PARCH_EDGE = rgb("#d6c49c"), rgb("#e6d7b4"), rgb("#bba878"), rgb("#6b5634")
RIVET, RIVET_HI = rgb("#8a6a3a"), rgb("#d9b46a")
OUT = rgb("#2b2433")
BAYER4 = [[0, 8, 2, 10], [12, 4, 14, 6], [3, 11, 1, 9], [15, 7, 13, 5]]


def _bevel_frame(size, border, fill_fn, edge_dark, edge_hi, edge_lo):
    c = Canvas(size, size)
    for y in range(size):
        for x in range(size):
            c.replace(x, y, fill_fn(x, y))
    c.frame(0, 0, size, size, UI_OUT)
    for i in range(1, border - 2):
        col = edge_hi if i == 1 else edge_dark
        c.hline(i, i, size - 2 * i, col)
        c.vline(i, i, size - 2 * i, col)
        c.hline(i, size - 1 - i, size - 2 * i, edge_lo)
        c.vline(size - 1 - i, i, size - 2 * i, edge_lo)
    c.frame(border - 2, border - 2, size - 2 * (border - 2), size - 2 * (border - 2), UI_OUT)
    return c


def stone_panel():
    """9-slice, 32x32, border 7."""
    rnd = random.Random(3)
    noise = {(x, y): rnd.random() for x in range(32) for y in range(32)}
    fill = lambda x, y: ST_MID if noise[(x, y)] > 0.86 else ST_DARK if noise[(x, y)] < 0.1 else ST
    c = _bevel_frame(32, 7, fill, rgb("#594d3b"), ST_HI, rgb("#1d1812"))
    for (x, y) in ((3, 3), (28, 3), (3, 28), (28, 28)):
        c.set(x, y, RIVET_HI)
        c.set(x + 1, y + 1, RIVET)
    return c


def parchment_panel():
    """9-slice chatbox, 32x32, border 7: wooden rim around parchment."""
    rnd = random.Random(5)
    noise = {(x, y): rnd.random() for x in range(32) for y in range(32)}

    def fill(x, y):
        n = noise[(x, y)]
        return PARCH_HI if n > 0.9 else PARCH_SH if n < 0.08 else PARCH

    c = _bevel_frame(32, 7, fill, rgb("#5a4229"), rgb("#8a6a42"), rgb("#3a2a18"))
    return c


def menu_panel():
    c = Canvas(12, 12)
    c.rect(0, 0, 12, 12, rgb("#000000", 235))
    c.frame(0, 0, 12, 12, rgb("#5d5447"))
    return c


def menu_header():
    c = Canvas(12, 12)
    c.rect(0, 0, 12, 12, rgb("#000000"))
    c.frame(0, 0, 12, 12, rgb("#5d5447"))
    c.rect(1, 1, 10, 10, rgb("#5d5447"))
    return c


def tooltip_panel():
    c = Canvas(8, 8)
    c.rect(0, 0, 8, 8, rgb("#ffffa0"))
    c.frame(0, 0, 8, 8, rgb("#000000"))
    return c


def dark_panel():
    """Translucent box for the objective tracker. 16x16, border 4."""
    c = Canvas(16, 16)
    c.rect(0, 0, 16, 16, rgb("#120e0a", 170))
    c.frame(0, 0, 16, 16, UI_OUT)
    c.frame(1, 1, 14, 14, rgb("#5d5447"))
    return c


def slot_hover():
    c = Canvas(28, 26)
    c.rect(1, 1, 26, 24, rgb("#ffffff", 28))
    c.frame(0, 0, 28, 26, rgb("#ffffff", 45))
    return c


def tab(active):
    c = Canvas(30, 26)
    base = rgb("#7a3226") if active else ST
    hi = rgb("#a14a38") if active else ST_HI
    c.rect(0, 0, 30, 26, base)
    c.frame(0, 0, 30, 26, UI_OUT)
    c.hline(1, 1, 28, hi)
    c.vline(1, 1, 24, hi)
    c.hline(1, 24, 28, rgb("#1d1812"))
    c.vline(28, 1, 24, rgb("#1d1812"))
    return c


def icon_backpack():
    c = Canvas(16, 16)
    bag, bag_hi, bag_sh = rgb("#8a5a32"), rgb("#b07a4a"), rgb("#5e3a1e")
    c.rect(3, 4, 10, 11, bag)
    c.vline(3, 4, 11, bag_hi)
    c.vline(12, 4, 11, bag_sh)
    c.rect(4, 2, 8, 3, bag_sh)
    c.rect(5, 7, 6, 4, bag_hi)
    c.rect(7, 8, 2, 2, rgb("#d4ad55"))
    c.outline(UI_OUT)
    return c


def icon_stats():
    c = Canvas(16, 16)
    for i, (h, col) in enumerate(((5, rgb("#d9443a")), (8, rgb("#e0b23a")), (11, rgb("#5ab44a")))):
        x = 2 + i * 4
        c.rect(x, 14 - h, 3, h, col)
        c.vline(x, 14 - h, h, mix(col, rgb("#ffffff"), 0.35))
    c.hline(1, 14, 14, rgb("#c9b48a"))
    c.outline(UI_OUT)
    return c


def icon_woodcutting():
    c = Canvas(16, 16)
    leaf, leaf_hi, leaf_sh = rgb("#5a9a4a"), rgb("#86c46a"), rgb("#3a6a34")
    c.ellipse(6, 6, 5, 5, leaf)
    c.ellipse(5, 5, 3, 3, leaf_hi)
    c.ellipse(8, 8, 3, 2.5, leaf_sh)
    c.rect(5, 10, 3, 5, rgb("#7a5232"))
    c.line(10, 14, 14, 6, rgb("#a3714a"))
    c.rect(12, 5, 3, 4, rgb("#c8d0da"))
    c.vline(14, 5, 4, rgb("#ffffff"))
    c.outline(UI_OUT)
    return c


def keycap():
    c = Canvas(11, 12)
    c.rect(0, 0, 11, 11, rgb("#1a1612"))
    c.rect(1, 0, 9, 9, rgb("#d9d2c4"))
    c.rect(1, 9, 9, 2, rgb("#8f877a"))
    c.hline(1, 0, 9, rgb("#f4efe6"))
    c.frame(0, 0, 11, 12, UI_OUT)
    return c


def portrait_frame():
    """9-slice frame drawn over dialogue portraits. 16x16, border 3."""
    c = Canvas(16, 16)
    c.frame(0, 0, 16, 16, UI_OUT)
    c.frame(1, 1, 14, 14, PARCH_EDGE)
    c.frame(2, 2, 12, 12, rgb("#8a6a42"))
    return c


def skill_tile():
    """OSRS-style stat tile background. 16x16, border 4."""
    c = Canvas(16, 16)
    c.rect(0, 0, 16, 16, rgb("#2e2720"))
    c.frame(0, 0, 16, 16, UI_OUT)
    c.hline(1, 1, 14, ST_HI)
    c.vline(1, 1, 14, ST_HI)
    c.hline(1, 14, 14, rgb("#1d1812"))
    c.vline(14, 1, 14, rgb("#1d1812"))
    return c


# ---------------------------------------------------------------- item icons (24x24)
def item_feather():
    c = Canvas(24, 24)
    vane, vane_hi, vane_sh = rgb("#f4fbff"), rgb("#ffffff"), rgb("#b8d8ee")
    glow = rgb("#8ff0ff")
    for i in range(16):
        x, y = 4 + i, 19 - i
        half = round(3.2 * (1 - abs(i - 8) / 9))
        for k in range(1, half + 1):
            c.set(x - k, y - k + 1, vane_sh if k == half else vane)
            c.set(x + k - 1, y + k, vane if k < half else vane_sh)
    c.line(3, 21, 19, 4, rgb("#9fb3c4"))
    c.line(2, 22, 5, 19, rgb("#d6b98a"))
    c.outline(OUT)
    for (x, y) in ((20, 3), (22, 8), (6, 12), (16, 15)):
        c.set(x, y, with_alpha(glow, 220))
    c.set(21, 4, vane_hi)
    return c


def item_axe():
    c = Canvas(24, 24)
    c.line(4, 21, 15, 6, rgb("#7a5232"))
    c.line(5, 21, 16, 6, rgb("#a3714a"))
    hi, base, shade = rgb("#f0c08a"), rgb("#c8803f"), rgb("#8a4f22")
    for i, w in enumerate((3, 5, 6, 7, 7, 6, 4)):
        y = 3 + i
        c.hline(14, y, w, base)
        c.set(14 + w - 1, y, hi)
        if i >= 5:
            c.hline(14, y, w - 1, shade)
    c.outline(OUT)
    return c


def item_logs():
    c = Canvas(24, 24)
    bark, bark_hi, bark_sh = rgb("#6e4b33"), rgb("#9a6c4a"), rgb("#4a3223")
    wood, ring = rgb("#d8a86e"), rgb("#a2733f")
    for (y, x0, x1) in ((14, 3, 18), (8, 6, 21)):
        for yy in range(y, y + 6):
            c.hline(x0, yy, x1 - x0, bark)
        c.hline(x0, y, x1 - x0, bark_hi)
        c.hline(x0, y + 5, x1 - x0, bark_sh)
        c.ellipse(x1, y + 3, 2.6, 3.1, wood)
        c.set(x1, y + 3, ring)
        c.set(x1, y + 2, ring)
    c.outline(OUT)
    return c


ITEM_ICONS = {"GaleFeather": item_feather, "BronzeAxe": item_axe, "Logs": item_logs}
