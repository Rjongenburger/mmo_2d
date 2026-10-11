"""Procedural sound effects (pure Python, mono 16-bit WAV)."""
import math
import random
import struct
import wave

RATE = 22050


class Biquad:
    """RBJ-cookbook band-pass / low-pass / high-pass filter with settable frequency."""

    def __init__(self, kind, freq, q=0.7):
        self.kind, self.q = kind, q
        self.x1 = self.x2 = self.y1 = self.y2 = 0.0
        self.set(freq)

    def set(self, freq):
        w0 = 2 * math.pi * max(20.0, min(freq, RATE * 0.45)) / RATE
        alpha = math.sin(w0) / (2 * self.q)
        cw = math.cos(w0)
        if self.kind == "lp":
            b0, b1, b2 = (1 - cw) / 2, 1 - cw, (1 - cw) / 2
        elif self.kind == "hp":
            b0, b1, b2 = (1 + cw) / 2, -(1 + cw), (1 + cw) / 2
        else:  # constant-peak band-pass
            b0, b1, b2 = alpha, 0.0, -alpha
        a0, a1, a2 = 1 + alpha, -2 * cw, 1 - alpha
        self.b0, self.b1, self.b2, self.a1, self.a2 = b0 / a0, b1 / a0, b2 / a0, a1 / a0, a2 / a0

    def __call__(self, x):
        y = self.b0 * x + self.b1 * self.x1 + self.b2 * self.x2 - self.a1 * self.y1 - self.a2 * self.y2
        self.x2, self.x1, self.y2, self.y1 = self.x1, x, self.y1, y
        return y


def _write(path, samples, gain=1.0):
    peak = max(1e-9, max(abs(s) for s in samples))
    scale = gain * 0.89 / peak
    with wave.open(path, "wb") as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(RATE)
        w.writeframes(b"".join(struct.pack("<h", int(max(-1, min(1, s * scale)) * 32767)) for s in samples))


def _loop_crossfade(samples, fade):
    """Folds the extra tail onto the head so the clip loops seamlessly."""
    body = samples[:-fade]
    for i in range(fade):
        t = i / fade
        body[i] = body[i] * t + samples[len(samples) - fade + i] * (1 - t)
    return body


def wind_loop(path, seconds=12):
    rnd = random.Random(1)
    n, fade = seconds * RATE, RATE
    lp, howl, howl2 = Biquad("lp", 600), Biquad("bp", 420, 9), Biquad("bp", 690, 12)
    out = []
    for i in range(n + fade):
        t = (i % n) / RATE
        gust = 0.55 + 0.25 * math.sin(2 * math.pi * t / seconds) + 0.15 * math.sin(2 * math.pi * 3 * t / seconds + 1.3) \
            + 0.1 * math.sin(2 * math.pi * 7 * t / seconds + 0.4)
        if i % 64 == 0:
            lp.set(250 + 900 * gust)
            howl.set(330 + 260 * gust)
            howl2.set(560 + 300 * gust)
        x = rnd.uniform(-1, 1)
        s = lp(x) * (0.6 + 0.6 * gust) + howl(x) * 2.6 * gust ** 2 + howl2(x) * 1.6 * gust ** 3
        out.append(s)
    _write(path, _loop_crossfade(out, fade), 0.8)


def portal(path, seconds=3.0):
    rnd = random.Random(3)
    n = int(seconds * RATE)
    bp = Biquad("bp", 200, 3)
    out = []
    for i in range(n):
        t = i / RATE
        p = t / seconds
        env = min(1, p / 0.55) ** 2 * (1 - max(0, (p - 0.8) / 0.2))
        if i % 32 == 0:
            bp.set(200 + 1800 * p ** 1.5)
        whoosh = bp(rnd.uniform(-1, 1)) * 2.2
        chord = sum(math.sin(2 * math.pi * f * (1 + 0.12 * p) * t + 0.4 * math.sin(2 * math.pi * 5.5 * t)) for f in (220, 330, 440))
        shimmer = math.sin(2 * math.pi * 1760 * t) * (0.5 + 0.5 * math.sin(2 * math.pi * 11 * t)) * 0.25
        out.append(env * (whoosh + chord * 0.18 + shimmer * p))
    _write(path, out, 0.9)


def pickup(path):
    n = int(0.18 * RATE)
    out = []
    for i in range(n):
        t = i / RATE
        f = 784 if t < 0.07 else 1175
        tt = t if t < 0.07 else t - 0.07
        env = math.exp(-tt * 28) * min(1, tt * 400)
        sq = 1 if math.sin(2 * math.pi * f * t) > 0 else -1
        out.append((math.sin(2 * math.pi * f * t) * 0.7 + sq * 0.15) * env)
    _write(path, out, 0.6)


def leap(path):
    rnd = random.Random(6)
    n = int(0.9 * RATE)
    bp = Biquad("bp", 400, 1.8)
    out = []
    for i in range(n):
        p = i / n
        if i % 32 == 0:
            bp.set(400 + 900 * math.sin(p * math.pi))
        env = math.sin(p * math.pi) ** 1.5
        out.append(bp(rnd.uniform(-1, 1)) * env)
    _write(path, out, 0.85)


def land(path):
    rnd = random.Random(7)
    n = int(0.25 * RATE)
    lp = Biquad("lp", 500)
    out = []
    for i in range(n):
        t = i / RATE
        out.append((lp(rnd.uniform(-1, 1)) * 2 + math.sin(2 * math.pi * 80 * t)) * math.exp(-t * 18))
    _write(path, out, 0.7)


def ui_click(path):
    n = int(0.05 * RATE)
    out = [math.sin(2 * math.pi * 1400 * i / RATE) * math.exp(-i / RATE * 120) for i in range(n)]
    _write(path, out, 0.4)


ALL = {
    "WindLoop": wind_loop, "Portal": portal, "Pickup": pickup, "Leap": leap, "Land": land, "UiClick": ui_click,
}


def fire_cast(path):
    rnd = random.Random(11)
    n = int(0.45 * RATE)
    bp, lp = Biquad("bp", 500, 1.2), Biquad("lp", 300)
    out = []
    for i in range(n):
        p = i / n
        if i % 32 == 0:
            bp.set(500 + 2200 * p)
        env = min(1, p / 0.08) * (1 - p) ** 1.4
        roar = lp(rnd.uniform(-1, 1)) * 2.5 * (1 - p)
        out.append((bp(rnd.uniform(-1, 1)) * 1.8 + roar) * env)
    _write(path, out, 0.85)


def fire_hit(path):
    rnd = random.Random(12)
    n = int(0.7 * RATE)
    lp = Biquad("lp", 900)
    out = []
    for i in range(n):
        t = i / RATE
        if i % 32 == 0:
            lp.set(900 * math.exp(-t * 4) + 120)
        boom = math.sin(2 * math.pi * (90 - 40 * min(1, t / 0.3)) * t) * math.exp(-t * 7)
        crackle = lp(rnd.uniform(-1, 1)) * 2.4 * math.exp(-t * 5)
        out.append(boom + crackle)
    _write(path, out, 0.9)


def caw(path):
    """Two harsh crow calls: a buzzy, pitch-dropping sawtooth through a nasal band-pass."""
    rnd = random.Random(13)
    n = int(0.62 * RATE)
    bp = Biquad("bp", 1400, 2.5)
    out, phase = [], 0.0
    for i in range(n):
        t = i / RATE
        local = t if t < 0.26 else t - 0.32
        on = (0 <= local < 0.24)
        f = 620 - 260 * (local / 0.24) + 25 * math.sin(2 * math.pi * 38 * t) if on else 0
        phase = (phase + f / RATE) % 1
        saw = (phase * 2 - 1) if on else 0
        env = (math.sin(min(1, local / 0.24) * math.pi) ** 0.6) if on else 0
        out.append(bp(saw + rnd.uniform(-0.25, 0.25) * on) * 3 * env)
    _write(path, out, 0.75)


def crow_die(path):
    rnd = random.Random(14)
    n = int(0.55 * RATE)
    bp = Biquad("bp", 1800, 2)
    lp = Biquad("lp", 400)
    out, phase = [], 0.0
    for i in range(n):
        t = i / RATE
        f = 900 * math.exp(-t * 3.5) + 200
        phase = (phase + f / RATE) % 1
        squawk = bp(phase * 2 - 1) * 2.5 * math.exp(-t * 6)
        puff = lp(rnd.uniform(-1, 1)) * 1.5 * math.exp(-max(0, t - 0.15) * 10) * (t > 0.15)
        out.append(squawk + puff)
    _write(path, out, 0.75)


def jump(path):
    rnd = random.Random(15)
    n = int(0.22 * RATE)
    bp = Biquad("bp", 300, 2)
    out = []
    for i in range(n):
        p = i / n
        if i % 16 == 0:
            bp.set(300 + 900 * p)
        out.append(bp(rnd.uniform(-1, 1)) * math.sin(p * math.pi) * 2 + math.sin(2 * math.pi * (300 + 300 * p) * i / RATE) * 0.15 * (1 - p))
    _write(path, out, 0.55)


def fly(path):
    rnd = random.Random(16)
    n = int(1.1 * RATE)
    bp = Biquad("bp", 300, 1.5)
    out = []
    for i in range(n):
        t = i / RATE
        p = i / n
        if i % 32 == 0:
            bp.set(300 + 1200 * math.sin(p * math.pi))
        swell = bp(rnd.uniform(-1, 1)) * 2 * math.sin(p * math.pi)
        chime = sum(math.sin(2 * math.pi * f * t) for f in (1320, 1760, 2640)) * 0.12 * math.exp(-t * 3)
        out.append(swell + chime)
    _write(path, out, 0.8)


def fall(path):
    rnd = random.Random(17)
    n = int(1.2 * RATE)
    bp = Biquad("bp", 600, 1.5)
    out, phase = [], 0.0
    for i in range(n):
        t = i / RATE
        p = i / n
        f = 1100 * (1 - p) + 180
        phase += f / RATE
        whistle = math.sin(2 * math.pi * phase) * 0.35 * (1 - p)
        if i % 32 == 0:
            bp.set(400 + 600 * (1 - p))
        wind = bp(rnd.uniform(-1, 1)) * 1.5 * min(1, p * 3)
        out.append((whistle + wind) * (1 - max(0, p - 0.8) / 0.2))
    _write(path, out, 0.75)


def hurt(path):
    n = int(0.22 * RATE)
    out = []
    for i in range(n):
        t = i / RATE
        f = 420 - 240 * min(1, t / 0.18)
        sq = 1 if math.sin(2 * math.pi * f * t) > 0 else -1
        out.append((sq * 0.5 + math.sin(2 * math.pi * 70 * t)) * math.exp(-t * 14))
    _write(path, out, 0.6)


def blip(path):
    """Dialogue "voice" tick; pitched per speaker in-engine."""
    n = int(0.045 * RATE)
    out = []
    for i in range(n):
        t = i / RATE
        sq = 1 if math.sin(2 * math.pi * 520 * t) > 0 else -1
        out.append((sq * 0.6 + math.sin(2 * math.pi * 1040 * t) * 0.3) * min(1, t * 900) * math.exp(-t * 55))
    _write(path, out, 0.35)


def _arpeggio(path, notes, step, length, square=False, gain=0.7):
    n = int(length * RATE)
    out = []
    for i in range(n):
        t = i / RATE
        s = 0.0
        for k, f in enumerate(notes):
            start = k * step
            if t < start:
                continue
            tt = t - start
            wave = math.sin(2 * math.pi * f * tt)
            if square:
                wave = 0.6 * (1 if wave > 0 else -1) + 0.4 * wave
            s += wave * math.exp(-tt * 3.2) * min(1, tt * 300)
        out.append(s)
    _write(path, out, gain)


def spell_learned(path):
    _arpeggio(path, [523.25, 659.25, 783.99, 1046.5, 1318.5], 0.11, 1.6)


def level_up(path):
    _arpeggio(path, [392.0, 523.25, 659.25, 783.99, 1046.5, 1046.5], 0.09, 1.5, square=True, gain=0.55)


def respawn(path):
    _arpeggio(path, [880.0, 1108.7, 1318.5, 1760.0], 0.07, 1.0, gain=0.5)


ALL.update({
    "FireCast": fire_cast, "FireHit": fire_hit, "Caw": caw, "CrowDie": crow_die, "Jump": jump, "Fly": fly,
    "Fall": fall, "Hurt": hurt, "Blip": blip, "SpellLearned": spell_learned, "LevelUp": level_up, "Respawn": respawn,
})
