"""OSRS-flavoured HUD pieces, item icons and skill icons."""
import math
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


ITEM_ICONS = {"GaleFeather": item_feather}


# ---------------------------------------------------------------- dialogue (non-OSRS look)
NIGHT_TOP, NIGHT_BOTTOM, NIGHT_EDGE = rgb("#2b3354"), rgb("#161a2c"), rgb("#0b0d16")
GOLD, GOLD_HI, GOLD_SH = rgb("#c9a14a"), rgb("#ffe08a"), rgb("#7a5f2a")


def dialogue_panel():
    """9-slice (32x32, border 8): deep night-blue glass with a thin gold inlay and rounded corners."""
    c = Canvas(32, 32)
    for y in range(32):
        col = mix(NIGHT_TOP, NIGHT_BOTTOM, min(1.0, max(0.0, (y - 4) / 22)))
        c.hline(0, y, 32, with_alpha(col, 238))
    c.frame(0, 0, 32, 32, NIGHT_EDGE)
    c.frame(2, 2, 28, 28, GOLD)
    c.hline(3, 2, 26, GOLD_HI)
    c.frame(3, 3, 26, 26, with_alpha(NIGHT_EDGE, 200))
    for (x, y) in ((0, 0), (31, 0), (0, 31), (31, 31), (1, 0), (0, 1), (30, 0), (31, 1), (0, 30), (1, 31), (31, 30), (30, 31)):
        c.replace(x, y, (0, 0, 0, 0))
    for (x, y) in ((1, 1), (30, 1), (1, 30), (30, 30)):
        c.replace(x, y, NIGHT_EDGE)
    for (x, y) in ((2, 2), (29, 2), (2, 29), (29, 29)):  # gold studs in the corners
        c.replace(x, y, GOLD_HI)
    return c


def name_plate():
    """9-slice (16x14, border 5) plate tinted per speaker in-engine (light fill so tints stay vivid)."""
    c = Canvas(16, 14)
    c.rect(1, 1, 14, 12, rgb("#ffffff"))
    c.hline(1, 11, 14, rgb("#c8c8c8"))
    c.hline(1, 12, 14, rgb("#a8a8a8"))
    c.hline(2, 1, 12, rgb("#ffffff"))
    c.frame(0, 0, 16, 14, NIGHT_EDGE)
    for (x, y) in ((0, 0), (15, 0), (0, 13), (15, 13)):
        c.replace(x, y, (0, 0, 0, 0))
    return c


def portrait_ring():
    """9-slice (16x16, border 4) gold ring drawn over dialogue portraits."""
    c = Canvas(16, 16)
    c.rect(0, 0, 16, 16, rgb("#11131f"))
    c.frame(0, 0, 16, 16, NIGHT_EDGE)
    c.frame(1, 1, 14, 14, GOLD)
    c.hline(2, 1, 12, GOLD_HI)
    c.frame(2, 2, 12, 12, GOLD_SH)
    for (x, y) in ((0, 0), (15, 0), (0, 15), (15, 15)):
        c.replace(x, y, (0, 0, 0, 0))
    for y in range(3, 13):  # clear the middle so the portrait shows through
        for x in range(3, 13):
            c.replace(x, y, (0, 0, 0, 0))
    return c


def portrait_backdrop():
    c = Canvas(8, 8)
    for y in range(8):
        c.hline(0, y, 8, mix(rgb("#3a2f5c"), rgb("#1b1830"), y / 7))
    return c


# ---------------------------------------------------------------- equipment / magic tabs
def icon_equipment():
    """Tab icon: a little helmet."""
    c = Canvas(16, 16)
    steel, steel_hi, steel_sh = rgb("#b8c0cc"), rgb("#eef2f6"), rgb("#6f7884")
    c.ellipse(8, 8, 6, 5.5, steel)
    c.rect(2, 8, 13, 5, steel)
    c.ellipse(6, 5, 2.5, 2, steel_hi)
    c.rect(4, 9, 9, 2, rgb("#1b1830"))
    c.vline(8, 9, 4, steel)
    c.hline(2, 12, 13, steel_sh)
    c.vline(8, 2, 6, rgb("#c94a4a"))
    c.outline(UI_OUT)
    return c


def icon_spellbook():
    """Tab icon: an open spellbook with a star."""
    c = Canvas(16, 16)
    cover, page, page_sh = rgb("#5a3fa8"), rgb("#f2e6c8"), rgb("#c9b48a")
    c.rect(1, 4, 14, 10, cover)
    c.rect(2, 3, 6, 9, page)
    c.rect(8, 3, 6, 9, page)
    c.vline(7, 3, 9, page_sh)
    c.vline(8, 3, 9, page_sh)
    for (x, y) in ((4, 6), (11, 5), (11, 8), (3, 9)):
        c.hline(x - 1, y, 3, page_sh)
    star = rgb("#ffb03a")
    c.set(11, 6, star)
    c.set(10, 7, star)
    c.set(12, 7, star)
    c.set(11, 7, rgb("#fff3b0"))
    c.set(11, 8, star)
    c.outline(UI_OUT)
    return c


def icon_magic_skill():
    """16x16 Magic skill icon: a pointed hat with a star."""
    c = Canvas(16, 16)
    hat, hat_hi, hat_sh = rgb("#4b48a8"), rgb("#6d6ad0"), rgb("#2c2a66")
    for y in range(2, 12):
        w = 1 + (y - 2)
        x0 = 8 - w // 2 + (1 if y < 5 else 0)
        c.hline(x0, y, w, hat)
        c.set(x0, y, hat_hi)
        c.set(x0 + w - 1, y, hat_sh)
    c.ellipse(8, 12.5, 7, 2, hat_sh)
    c.ellipse(8, 12, 6, 1.4, hat)
    c.hline(4, 10, 9, rgb("#d9a93e"))
    c.set(8, 6, rgb("#ffe08a"))
    c.set(7, 7, rgb("#ffe08a"))
    c.set(9, 7, rgb("#ffe08a"))
    c.set(8, 8, rgb("#ffe08a"))
    c.outline(UI_OUT)
    return c


def equip_slot_frame():
    """28x28 square slot frame for the worn-equipment tab (9-slice, border 4)."""
    c = Canvas(28, 28)
    c.rect(0, 0, 28, 28, rgb("#2a231b"))
    c.frame(0, 0, 28, 28, UI_OUT)
    c.hline(1, 1, 26, ST_HI)
    c.vline(1, 1, 26, ST_HI)
    c.hline(1, 26, 26, rgb("#1d1812"))
    c.vline(26, 1, 26, rgb("#1d1812"))
    return c


GHOST = rgb("#8a7a64", 150)


def _ghost(draw):
    c = Canvas(24, 24)
    draw(c)
    return c


SLOT_SILHOUETTES = {
    "Head": lambda c: (c.ellipse(12, 11, 7, 6.5, GHOST), c.rect(5, 11, 15, 6, GHOST)),
    "Cape": lambda c: [c.hline(12 - (4 + y // 3), y, 2 * (4 + y // 3), GHOST) for y in range(3, 21)],
    "Neck": lambda c: ([c.set(round(12 + 6 * math.cos(a / 20 * math.pi)), round(6 + 8 * math.sin(a / 20 * math.pi)), GHOST) for a in range(21)],
                       c.ellipse(12, 16, 2.5, 3, GHOST)),
    "Ammo": lambda c: [(c.line(6 + i * 4, 20, 10 + i * 4, 4, GHOST), c.set(10 + i * 4, 3, GHOST)) for i in range(3)],
    "Weapon": lambda c: (c.line(5, 19, 17, 5, GHOST), c.line(6, 19, 18, 5, GHOST), c.hline(5, 15, 6, GHOST), c.rect(4, 19, 3, 3, GHOST)),
    "Body": lambda c: (c.rect(7, 5, 10, 15, GHOST), c.rect(3, 5, 4, 9, GHOST), c.rect(17, 5, 4, 9, GHOST)),
    "Shield": lambda c: (c.rect(6, 4, 12, 9, GHOST), [c.hline(6 + y // 2, 13 + y, 12 - y, GHOST) for y in range(8)]),
    "Legs": lambda c: (c.rect(7, 3, 10, 5, GHOST), c.rect(7, 8, 4, 13, GHOST), c.rect(13, 8, 4, 13, GHOST)),
    "Hands": lambda c: (c.rect(7, 8, 9, 9, GHOST), c.rect(7, 4, 2, 5, GHOST), c.rect(10, 3, 2, 5, GHOST), c.rect(13, 4, 2, 5, GHOST), c.rect(16, 11, 3, 3, GHOST)),
    "Feet": lambda c: (c.rect(4, 10, 6, 8, GHOST), c.rect(4, 16, 9, 3, GHOST), c.rect(14, 10, 6, 8, GHOST), c.rect(14, 16, 9, 3, GHOST)),
    "Ring": lambda c: ([c.set(round(12 + 6 * math.cos(a / 16 * math.pi)), round(13 + 6 * math.sin(a / 16 * math.pi)), GHOST) for a in range(32)],
                       c.ellipse(12, 6, 2.5, 2, GHOST)),
}


def slot_silhouette(slot):
    return _ghost(SLOT_SILHOUETTES[slot])


def spell_icon(kind, locked=False):
    """24x24 spellbook icons."""
    c = Canvas(24, 24)
    if kind == "FireBolt":
        fire = [rgb("#fff8d6"), rgb("#ffe27a"), rgb("#ffb03a"), rgb("#f2671f"), rgb("#b8321a")]
        for i in range(9):
            c.line(4 + i // 3, 20 - i // 2, 11 + i // 2, 12 - i // 3, fire[3 + (i % 2)])
        c.ellipse(15, 9, 5.5, 5.5, fire[3])
        c.ellipse(15.5, 8.5, 4, 4, fire[2])
        c.ellipse(16, 8, 2.4, 2.4, fire[1])
        c.set(16, 7, fire[0])
        c.set(17, 7, fire[0])
    elif kind == "Gust":
        col = rgb("#bfefff")
        for r, start in ((8, 0.2), (5.5, 1.4), (3, 2.6)):
            for a in range(28):
                ang = start + a / 28 * math.pi * 1.4
                c.set(round(12 + math.cos(ang) * r), round(12 + math.sin(ang) * r), col)
    elif kind == "Frost":
        col, hi = rgb("#8fd8ff"), rgb("#eaffff")
        for k in range(6):
            ang = k / 6 * math.tau
            c.line(12, 12, round(12 + math.cos(ang) * 9), round(12 + math.sin(ang) * 9), col)
        c.ellipse(12, 12, 2.5, 2.5, hi)
    elif kind == "Mend":
        col, hi = rgb("#62d66a"), rgb("#c8ffb8")
        c.rect(9, 4, 6, 16, col)
        c.rect(4, 9, 16, 6, col)
        c.rect(10, 5, 2, 5, hi)
    c.outline(OUT)
    if locked:
        for y in range(24):
            for x in range(24):
                p = c.get(x, y)
                if p[3]:
                    g = (p[0] + p[1] + p[2]) // 3 // 2 + 20
                    c.replace(x, y, (g, g, g + 6, p[3]))
    return c


def spell_frame(active):
    """26x26 frame behind a spell icon; gold when it is the active spell."""
    c = Canvas(26, 26)
    if active:
        c.frame(0, 0, 26, 26, GOLD)
        c.frame(1, 1, 24, 24, with_alpha(GOLD_HI, 160))
        c.rect(2, 2, 22, 22, with_alpha(GOLD, 45))
    else:
        c.rect(1, 1, 24, 24, rgb("#000000", 60))
    return c


# ---------------------------------------------------------------- clothes (24x24)
def item_nightshirt():
    c = Canvas(24, 24)
    cloth, sh, deep = rgb("#ece0c4"), rgb("#cdbb93"), rgb("#a7926a")
    c.rect(7, 4, 10, 17, cloth)
    c.rect(2, 5, 5, 9, cloth)
    c.rect(17, 5, 5, 9, cloth)
    c.vline(16, 5, 16, sh)
    c.hline(7, 20, 10, deep)
    c.rect(10, 4, 4, 3, rgb("#f3cba5"))
    c.vline(12, 7, 4, rgb("#8a6a42"))
    c.outline(OUT)
    return c


def item_trousers():
    c = Canvas(24, 24)
    cloth, sh = rgb("#6e5640"), rgb("#4f3d2d")
    c.rect(6, 3, 12, 5, cloth)
    c.rect(6, 8, 5, 13, cloth)
    c.rect(13, 8, 5, 13, cloth)
    c.vline(10, 8, 13, sh)
    c.vline(17, 3, 18, sh)
    c.hline(6, 3, 12, rgb("#8a6a42"))
    c.outline(OUT)
    return c


def item_slippers():
    c = Canvas(24, 24)
    wool, wool_hi, sole = rgb("#b04a4a"), rgb("#d9786a"), rgb("#4a3628")
    for x0, y0 in ((3, 7), (12, 11)):
        c.ellipse(x0 + 4.5, y0 + 3, 4.5, 3.2, wool)
        c.ellipse(x0 + 3.5, y0 + 2.2, 2.2, 1.4, wool_hi)
        c.hline(x0 + 1, y0 + 6, 8, sole)
    c.outline(OUT)
    return c


ITEM_ICONS.update({"Nightshirt": item_nightshirt, "LinenTrousers": item_trousers, "WoolSlippers": item_slippers})
