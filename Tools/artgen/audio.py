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


def fire_loop(path, seconds=6):
    rnd = random.Random(2)
    n, fade = seconds * RATE, RATE // 2
    rumble, crack_hp = Biquad("lp", 180), Biquad("hp", 1800)
    out, crackle, crackle_amp = [], 0, 0.0
    for i in range(n + fade):
        s = rumble(rnd.uniform(-1, 1)) * 1.4
        if crackle <= 0 and rnd.random() < 14 / RATE:
            crackle, crackle_amp = rnd.randrange(30, 180), rnd.uniform(0.3, 1.0)
        if crackle > 0:
            s += crack_hp(rnd.uniform(-1, 1)) * crackle_amp * (crackle / 180)
            crackle -= 1
        out.append(s)
    _write(path, _loop_crossfade(out, fade), 0.7)


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


def chop(path):
    rnd = random.Random(4)
    n = int(0.32 * RATE)
    hp = Biquad("hp", 1200)
    out = []
    for i in range(n):
        t = i / RATE
        click = hp(rnd.uniform(-1, 1)) * math.exp(-t * 90)
        f = 190 - 70 * min(1, t / 0.08)
        body = math.sin(2 * math.pi * f * t) * math.exp(-t * 22)
        knock = math.sin(2 * math.pi * 520 * t) * math.exp(-t * 60) * 0.4
        out.append(click * 0.9 + body + knock)
    _write(path, out, 0.95)


def tree_fall(path):
    rnd = random.Random(5)
    n = int(1.4 * RATE)
    lp, thud_lp = Biquad("lp", 900), Biquad("lp", 200)
    out, phase = [], 0.0
    for i in range(n):
        t = i / RATE
        s = 0.0
        if t < 0.75:  # creak: wobbling sawtooth through a low-pass
            f = 95 + 35 * math.sin(2 * math.pi * 3 * t) + rnd.uniform(-4, 4)
            phase = (phase + f / RATE) % 1
            s += lp(phase * 2 - 1) * min(1, t / 0.1) * (1 - t / 0.75) * 0.8
        if t > 0.8:  # thud
            tt = t - 0.8
            s += (math.sin(2 * math.pi * 62 * tt) + thud_lp(rnd.uniform(-1, 1)) * 2.5) * math.exp(-tt * 9)
        out.append(s)
    _write(path, out, 0.95)


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


def ignite(path):
    rnd = random.Random(8)
    n = int(0.8 * RATE)
    lp = Biquad("lp", 300)
    out = []
    for i in range(n):
        p = i / n
        if i % 32 == 0:
            lp.set(300 + 2500 * p * (1 - p) * 2)
        env = min(1, p / 0.15) * (1 - p) ** 1.3
        out.append(lp(rnd.uniform(-1, 1)) * env)
    _write(path, out, 0.8)


def ui_click(path):
    n = int(0.05 * RATE)
    out = [math.sin(2 * math.pi * 1400 * i / RATE) * math.exp(-i / RATE * 120) for i in range(n)]
    _write(path, out, 0.4)


ALL = {
    "WindLoop": wind_loop, "FireLoop": fire_loop, "Portal": portal, "Chop": chop, "TreeFall": tree_fall,
    "Pickup": pickup, "Leap": leap, "Land": land, "Ignite": ignite, "UiClick": ui_click,
}
