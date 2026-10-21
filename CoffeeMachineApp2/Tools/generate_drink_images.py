from PIL import Image, ImageDraw, ImageFilter, ImageChops
import math, random

S = 3
W, H = 600, 450
rnd = random.Random(11)

def C(v): return int(round(v * S))
def lerp(a, b, t): return a + (b - a) * t
def lerpc(c0, c1, t): return tuple(int(round(lerp(c0[i], c1[i], t))) for i in range(len(c0)))
def blank(): return Image.new("RGBA", (W * S, H * S), (0, 0, 0, 0))
def newmask(): return Image.new("L", (W * S, H * S), 0)

def m_ellipse(cx, cy, rx, ry):
    m = newmask(); ImageDraw.Draw(m).ellipse([C(cx - rx), C(cy - ry), C(cx + rx), C(cy + ry)], fill=255); return m
def m_poly(pts):
    m = newmask(); ImageDraw.Draw(m).polygon([(C(x), C(y)) for x, y in pts], fill=255); return m
def m_rrect(x0, y0, x1, y1, r):
    m = newmask(); ImageDraw.Draw(m).rounded_rectangle([C(x0), C(y0), C(x1), C(y1)], radius=C(r), fill=255); return m
def m_line(pts, width, cap=True):
    m = newmask(); d = ImageDraw.Draw(m)
    p = [(C(x), C(y)) for x, y in pts]
    d.line(p, fill=255, width=C(width), joint="curve")
    if cap:
        r = C(width) / 2
        for (x, y) in (p[0], p[-1]):
            d.ellipse([x - r, y - r, x + r, y + r], fill=255)
    return m
def m_union(*ms):
    out = ms[0]
    for m in ms[1:]: out = ImageChops.lighter(out, m)
    return out
def m_sub(a, b): return ImageChops.subtract(a, b)
def m_inter(a, b): return ImageChops.multiply(a, b)
def soft(m, r): return m.filter(ImageFilter.GaussianBlur(C(r)))
def scale_mask(m, k): return m.point(lambda v: int(v * k))

def solid(color):
    if len(color) == 3: color = color + (255,)
    return Image.new("RGBA", (W * S, H * S), color)

def vgrad(y0, y1, c0, c1):
    img = Image.new("RGBA", (W * S, H * S)); d = ImageDraw.Draw(img)
    a, b = C(y0), C(y1)
    for y in range(H * S):
        t = 0 if y <= a else (1 if y >= b else (y - a) / max(1, b - a))
        c = lerpc(c0 if len(c0) == 4 else c0 + (255,), c1 if len(c1) == 4 else c1 + (255,), t)
        d.line([(0, y), (W * S, y)], fill=c)
    return img

def hgrad(x0, x1, stops):
    img = Image.new("RGBA", (W * S, H * S)); d = ImageDraw.Draw(img)
    a, b = C(x0), C(x1)
    st = [(t, c if len(c) == 4 else c + (255,)) for t, c in stops]
    for x in range(W * S):
        t = 0 if x <= a else (1 if x >= b else (x - a) / max(1, b - a))
        for i in range(len(st) - 1):
            if st[i][0] <= t <= st[i + 1][0]:
                u = (t - st[i][0]) / max(1e-6, st[i + 1][0] - st[i][0])
                c = lerpc(st[i][1], st[i + 1][1], u); break
        else:
            c = st[-1][1]
        d.line([(x, 0), (x, H * S)], fill=c)
    return img

def put(base, mask, src, alpha=1.0):
    s = src.copy()
    a = ImageChops.multiply(s.getchannel("A"), mask)
    if alpha < 1: a = scale_mask(a, alpha)
    s.putalpha(a)
    base.alpha_composite(s)

def band(y0, y1): return m_poly([(0, y0), (W, y0), (W, y1), (0, y1)])

# ---------- parts ----------
def shadow(base, cx, cy, rx, ry, alpha=0.28):
    put(base, soft(m_ellipse(cx, cy, rx, ry), 9), solid((95, 70, 50)), alpha)

def saucer(base, cx, cy, rx, ry, color=(246, 246, 248)):
    shadow(base, cx + 6, cy + ry * 0.55, rx * 1.02, ry * 0.9)
    put(base, m_ellipse(cx, cy + 4, rx, ry), solid((190, 190, 196)))
    put(base, m_ellipse(cx, cy, rx, ry), hgrad(cx - rx, cx + rx, [(0, (232, 232, 236)), (0.35, color), (1, (205, 205, 212))]))
    put(base, m_ellipse(cx, cy + ry * 0.05, rx * 0.66, ry * 0.62), hgrad(cx - rx, cx + rx, [(0, (215, 215, 220)), (0.5, (240, 240, 243)), (1, (200, 200, 207))]))

def body_mask(cx, top_y, bot_y, top_rx, bot_rx, bot_ry, curve=1.0):
    L, R, n = [], [], 40
    for i in range(n + 1):
        t = i / n; y = top_y + (bot_y - top_y) * t
        hw = lerp(top_rx, bot_rx, t ** curve)
        L.append((cx - hw, y)); R.append((cx + hw, y))
    return m_union(m_poly(L + R[::-1]), m_ellipse(cx, bot_y, bot_rx, bot_ry))

def ring_mask(cx, cy, rx, ry, thick):
    return m_sub(m_ellipse(cx, cy, rx, ry), m_ellipse(cx, cy, rx - thick, ry - thick))

def cup(base, cx, top_y, bot_y, top_rx, bot_rx, rim_ry, color=((236, 236, 240), (255, 255, 255), (196, 196, 204)), handle=True):
    ccol = lambda i: color[i]
    shade = hgrad(cx - top_rx, cx + top_rx, [(0, ccol(0)), (0.28, ccol(1)), (0.62, ccol(1)), (1, ccol(2))])
    if handle:
        hr = (bot_y - top_y) * 0.30
        hcx = cx + top_rx * 0.86 + hr * 0.45; hcy = top_y + (bot_y - top_y) * 0.36
        put(base, ring_mask(hcx, hcy, hr, hr * 1.05, hr * 0.38), shade)
    bm = body_mask(cx, top_y, bot_y, top_rx, bot_rx, rim_ry * 0.9, curve=2.1)
    put(base, bm, shade)
    put(base, m_ellipse(cx - top_rx * 0.45, (top_y + bot_y) / 2 + 6, 5, (bot_y - top_y) * 0.28), solid((255, 255, 255)), 0.55)
    put(base, m_ellipse(cx, top_y, top_rx, rim_ry), hgrad(cx - top_rx, cx + top_rx, [(0, ccol(0)), (0.4, ccol(1)), (1, ccol(2))]))
    put(base, m_ellipse(cx, top_y + rim_ry * 0.10, top_rx * 0.93, rim_ry * 0.86), vgrad(top_y - rim_ry, top_y + rim_ry, (205, 205, 212), (235, 235, 240)))

def liquid_in_cup(base, cx, top_y, top_rx, rim_ry, top_color, edge_color=None):
    ex, ey = top_rx * 0.88, rim_ry * 0.74
    cy = top_y + rim_ry * 0.20
    if edge_color: put(base, m_ellipse(cx, cy, ex, ey), solid(edge_color))
    put(base, m_ellipse(cx, cy + 1, ex * 0.94, ey * 0.92), vgrad(cy - ey, cy + ey, lerpc(top_color, (255, 255, 255), 0.10), lerpc(top_color, (0, 0, 0), 0.18)))
    return cx, cy, ex * 0.94, ey * 0.92

def tulip(base, cx, cy, rx, ry, art, n=4):
    for i in range(n):
        t = i / (n - 1)
        r = lerp(0.82, 0.38, t)
        yy = cy + ry * lerp(0.35, -0.50, t)
        put(base, m_ellipse(cx, yy, rx * r * 0.78, ry * r * 0.62), solid(art), 0.96)
    put(base, m_line([(cx, cy + ry * 0.7), (cx, cy - ry * 0.75)], 3.0), solid(lerpc(art, (120, 80, 50), 0.35)), 0.55)

def rosette(base, cx, cy, rx, ry, art):
    for i in range(5):
        t = i / 4
        put(base, m_ellipse(cx, cy + ry * (0.32 - 0.1 * i), rx * (0.78 - 0.11 * i), ry * (0.40 - 0.045 * i)), solid(lerpc(art, (255, 250, 240), 0.0)), 0.9)
        put(base, m_ellipse(cx, cy + ry * (0.32 - 0.1 * i), rx * (0.70 - 0.11 * i), ry * (0.31 - 0.04 * i)), solid(lerpc((245, 232, 214), art, 0.15)), 0.9)
    put(base, m_line([(cx, cy + ry * 0.55), (cx, cy - ry * 0.5)], 2.4), solid(lerpc(art, (110, 70, 40), 0.4)), 0.7)

def glass(base, cx, top_y, bot_y, top_rx, bot_rx, rim_ry, layers, handle=False, foam=None):
    bot_ry = rim_ry * 0.9
    bm = body_mask(cx, top_y, bot_y, top_rx, bot_rx, bot_ry, 1.0)
    if handle:
        hr = (bot_y - top_y) * 0.26
        hcx = cx + (top_rx + bot_rx) / 2 + hr * 0.4; hcy = top_y + (bot_y - top_y) * 0.42
        put(base, ring_mask(hcx, hcy, hr, hr * 1.1, 7), solid((200, 214, 224)), 0.75)
    shadow(base, cx + 4, bot_y + 6, bot_rx * 1.1, 8, 0.22)
    put(base, bm, solid((226, 236, 242)), 0.45)
    inner = body_mask(cx, top_y + 3, bot_y - 10, top_rx - 5, bot_rx - 6, bot_ry - 2, 1.0)
    total = (bot_y - 10) - (top_y + 3)
    cursor = bot_y - 10
    for frac, c0, c1 in layers:
        y_top = cursor - frac * total
        put(base, m_inter(inner, band(y_top, cursor + 1)), vgrad(y_top, cursor, c1, c0))
        cursor = y_top
    top_col = layers[-1][2]
    # surface
    hw = lerp(top_rx - 5, bot_rx - 6, (cursor - top_y) / (bot_y - top_y))
    put(base, m_ellipse(cx, cursor, hw, rim_ry * 0.8), vgrad(cursor - rim_ry, cursor + rim_ry, lerpc(top_col, (255, 255, 255), 0.35), lerpc(top_col, (255, 255, 255), 0.05)))
    # glass walls
    edge = m_sub(bm, inner)
    put(base, edge, solid((196, 212, 222)), 0.75)
    put(base, m_ellipse(cx, bot_y - 5, bot_rx - 8, 6), solid((255, 255, 255)), 0.35)
    # highlights
    L = [(cx - top_rx + 12, top_y + 18), (cx - top_rx + 22, top_y + 18), (cx - bot_rx + 20, bot_y - 26), (cx - bot_rx + 12, bot_y - 26)]
    put(base, soft(m_poly(L), 1.0), solid((255, 255, 255)), 0.75)
    Rr = [(cx + top_rx - 16, top_y + 24), (cx + top_rx - 10, top_y + 24), (cx + bot_rx - 14, bot_y - 40), (cx + bot_rx - 19, bot_y - 40)]
    put(base, soft(m_poly(Rr), 1.0), solid((255, 255, 255)), 0.4)
    # rim
    put(base, ring_mask(cx, top_y, top_rx, rim_ry, 2.2), solid((200, 214, 224)), 0.9)
    return cursor

def steam(base, cx, y0, spread, height, n=3, alpha=0.30):
    for i in range(n):
        x0 = cx + (i - (n - 1) / 2) * spread
        pts = []
        for k in range(24):
            t = k / 23
            pts.append((x0 + math.sin(t * 6.0 + i * 1.7) * 9 * (0.4 + t), y0 - t * height * (0.85 + 0.12 * ((i + 1) % 2))))
        m = soft(m_line(pts, 8), 5)
        put(base, m, solid((170, 170, 178)), alpha)

def bean(base, x, y, r, ang=0.0):
    pts = []
    for k in range(40):
        a = k / 40 * 2 * math.pi
        px, py = math.cos(a) * r, math.sin(a) * r * 0.68
        pts.append((x + px * math.cos(ang) - py * math.sin(ang), y + px * math.sin(ang) + py * math.cos(ang)))
    put(base, soft(m_ellipse(x + 3, y + 4, r, r * 0.55), 2), solid((60, 40, 25)), 0.3)
    m = m_poly(pts)
    put(base, m, vgrad(y - r, y + r, (126, 82, 50), (66, 40, 24)))
    c = []
    for k in range(12):
        t = k / 11 - 0.5
        px, py = t * r * 1.7, math.sin(t * 3.2) * r * 0.18
        c.append((x + px * math.cos(ang) - py * math.sin(ang), y + px * math.sin(ang) + py * math.cos(ang)))
    put(base, m_line(c, 1.8), solid((38, 22, 12)), 0.85)

def cream_swirl(base, cx, base_y, rx, h, color=(255, 252, 248)):
    spec = [(rx, h * 0.26, base_y), (rx * 0.78, h * 0.24, base_y - h * 0.26), (rx * 0.54, h * 0.22, base_y - h * 0.48), (rx * 0.30, h * 0.18, base_y - h * 0.66)]
    for (r, ry, y) in spec:
        put(base, m_ellipse(cx + 2, y + 4, r, ry), solid((222, 210, 198)), 0.9)
        put(base, m_ellipse(cx, y, r, ry), vgrad(y - ry, y + ry, color, lerpc(color, (236, 226, 214), 0.8)))
    tip_y = base_y - h * 0.95
    put(base, m_poly([(cx - rx * 0.16, base_y - h * 0.72), (cx + rx * 0.16, base_y - h * 0.72), (cx + rx * 0.03, tip_y)]), solid(color))

def drizzle(base, x0, x1, y, amp, color=(70, 38, 24), n=7, w=2.6):
    pts = []
    for k in range(n * 6 + 1):
        t = k / (n * 6)
        pts.append((lerp(x0, x1, t), y + math.sin(t * math.pi * n) * amp))
    put(base, soft(m_line(pts, w), 0.5), solid(color), 0.9)

def dust(base, cx, cy, rx, ry, n=90, color=(88, 52, 30)):
    for _ in range(n):
        a = rnd.random() * 2 * math.pi; r = math.sqrt(rnd.random())
        x, y = cx + math.cos(a) * rx * r, cy + math.sin(a) * ry * r
        put(base, m_ellipse(x, y, rnd.uniform(0.7, 1.7), rnd.uniform(0.6, 1.3)), solid(color), rnd.uniform(0.35, 0.8))

def marshmallow(base, x, y, s, ang=0):
    put(base, soft(m_rrect(x - s + 2, y - s * 0.7 + 4, x + s + 2, y + s * 0.7 + 4, 5), 2), solid((120, 80, 60)), 0.35)
    put(base, m_rrect(x - s, y - s * 0.7, x + s, y + s * 0.7, 6), vgrad(y - s, y + s, (255, 250, 248), (244, 222, 226)))
    put(base, m_rrect(x - s * 0.85, y - s * 0.62, x + s * 0.2, y - s * 0.1, 4), solid((255, 255, 255)), 0.7)

def finish(img, name):
    small = img.resize((W, H), Image.LANCZOS)
    bg = Image.new("RGBA", (W, H), (255, 255, 255, 255)); bg.alpha_composite(small)
    bg.convert("RGB").save(name, optimize=True)

# ---------- drinks ----------
def espresso():
    b = blank(); cx = 300
    saucer(b, cx, 372, 175, 40)
    cup(b, cx, 262, 360, 92, 52, 26)
    liquid_in_cup(b, cx, 262, 92, 26, (84, 46, 22), edge_color=(176, 112, 58))
    put(b, m_ellipse(cx - 12, 268, 34, 8), solid((200, 140, 80)), 0.45)
    for (x, y, a) in [(150, 392, 0.4), (176, 404, -0.5), (455, 395, 1.0)]: bean(b, x, y, 13, a)
    steam(b, cx, 230, 26, 120)
    finish(b, "coffee1.png")

def americano():
    b = blank(); cx = 300
    saucer(b, cx, 380, 195, 42)
    cup(b, cx, 228, 366, 122, 76, 33)
    liquid_in_cup(b, cx, 228, 122, 33, (58, 34, 20), edge_color=(150, 96, 52))
    put(b, m_ellipse(cx - 25, 232, 48, 9), solid((190, 130, 76)), 0.30)
    for (x, y, a) in [(130, 402, 0.2), (468, 400, -0.7)]: bean(b, x, y, 13, a)
    steam(b, cx, 196, 34, 125)
    finish(b, "coffee2.png")

def cappuccino():
    b = blank(); cx = 300
    saucer(b, cx, 378, 200, 44)
    cup(b, cx, 232, 364, 142, 80, 38)
    x, y, rx, ry = liquid_in_cup(b, cx, 232, 142, 38, (238, 224, 204), edge_color=(176, 120, 78))
    rosette(b, x, y, rx, ry, (150, 100, 62))
    dust(b, x, y, rx * 0.85, ry * 0.8, 40, (92, 56, 34))
    for (px, py, a) in [(122, 405, 0.5), (140, 392, -0.3), (470, 402, 0.9)]: bean(b, px, py, 13, a)
    steam(b, cx, 206, 40, 115, alpha=0.22)
    finish(b, "coffee3.png")

def latte():
    b = blank(); cx = 300
    saucer(b, cx, 382, 150, 32)
    glass(b, cx, 130, 372, 96, 78, 28, [
        (0.60, (172, 118, 76), (206, 156, 112)),
        (0.28, (212, 168, 124), (232, 204, 168)),
        (0.12, (244, 232, 214), (250, 243, 230)),
    ], handle=True)
    steam(b, cx, 112, 30, 90, alpha=0.22)
    finish(b, "coffee4.png")

def macchiato():
    b = blank(); cx = 290
    saucer(b, cx, 384, 125, 28)
    glass(b, cx, 90, 374, 70, 58, 22, [
        (0.40, (236, 214, 184), (252, 246, 236)),
        (0.24, (88, 50, 28), (170, 112, 66)),
        (0.18, (190, 140, 96), (236, 214, 184)),
        (0.18, (246, 236, 220), (252, 247, 238)),
    ])
    put(b, m_line([(cx + 98, 330), (cx + 98, 150)], 11), solid((230, 235, 238)), 0.0)
    finish(b, "coffee5.png")

def mocha():
    b = blank(); cx = 300
    saucer(b, cx, 384, 150, 32)
    glass(b, cx, 190, 374, 94, 78, 26, [
        (0.20, (60, 30, 20), (84, 44, 28)),
        (0.30, (110, 66, 38), (150, 96, 58)),
        (0.30, (190, 142, 98), (222, 184, 142)),
        (0.12, (232, 210, 182), (244, 228, 206)),
    ], handle=True)
    cream_swirl(b, cx, 196, 84, 74)
    drizzle(b, cx - 66, cx + 66, 168, 4, (70, 38, 24), n=6)
    dust(b, cx, 152, 36, 14, 50)
    steam(b, cx, 100, 26, 70, alpha=0.2)
    finish(b, "coffee6.png")

def flatwhite():
    b = blank(); cx = 300
    saucer(b, cx, 376, 185, 40)
    cup(b, cx, 246, 362, 116, 70, 32)
    x, y, rx, ry = liquid_in_cup(b, cx, 246, 116, 32, (196, 140, 92), edge_color=(160, 104, 62))
    tulip(b, x, y, rx, ry, (248, 240, 228), n=4)
    for (px, py, a) in [(138, 404, 0.3), (462, 398, -0.6)]: bean(b, px, py, 13, a)
    steam(b, cx, 214, 32, 110, alpha=0.22)
    finish(b, "coffee7.png")

def raf():
    b = blank(); cx = 300
    saucer(b, cx, 384, 150, 32, color=(244, 240, 232))
    glass(b, cx, 150, 376, 84, 72, 25, [
        (0.30, (214, 178, 130), (232, 204, 160)),
        (0.45, (232, 204, 160), (244, 226, 194)),
        (0.25, (248, 240, 224), (253, 249, 240)),
    ])
    # vanilla pod (left) and sugar cubes (right), outside the saucer
    put(b, soft(m_line([(96, 414), (150, 408), (198, 414)], 9), 1.5), solid((60, 40, 25)), 0.25)
    put(b, m_line([(92, 410), (146, 403), (196, 410)], 8), solid((54, 34, 22)))
    put(b, m_line([(98, 407), (146, 400), (190, 406)], 2), solid((130, 96, 66)), 0.55)
    for (sx, sy) in [(486, 404), (518, 398)]:
        put(b, soft(m_rrect(sx - 15, sy - 11, sx + 17, sy + 17, 5), 2), solid((110, 90, 70)), 0.3)
        put(b, m_rrect(sx - 16, sy - 16, sx + 16, sy + 14, 5), vgrad(sy - 16, sy + 14, (255, 255, 255), (228, 228, 232)))
        put(b, m_rrect(sx - 16, sy - 16, sx + 16, sy - 6, 5), solid((255, 255, 255)), 0.9)
    steam(b, cx, 130, 24, 90, alpha=0.2)
    finish(b, "coffee8.png")

def hotchoc():
    b = blank(); cx = 300
    saucer(b, cx, 380, 190, 42)
    red = ((205, 108, 84), (232, 130, 102), (156, 70, 52))
    cup(b, cx, 238, 364, 126, 78, 34, color=red)
    x, y, rx, ry = liquid_in_cup(b, cx, 238, 126, 34, (78, 42, 28), edge_color=(120, 70, 46))
    cream_swirl(b, cx, 246, 78, 70)
    for (mx, my, s) in [(cx - 96, 236, 17), (cx + 100, 240, 16), (cx + 64, 258, 14)]: marshmallow(b, mx, my, s)
    drizzle(b, cx - 58, cx + 58, 214, 3.5, (86, 48, 30), n=6)
    dust(b, cx, 226, 40, 10, 45, (96, 56, 34))
    put(b, m_line([(468, 408), (524, 380)], 11), solid((140, 84, 50)))
    put(b, m_line([(468, 408), (524, 380)], 3), solid((190, 130, 84)), 0.7)
    steam(b, cx, 140, 34, 90, alpha=0.2)
    finish(b, "coffee9.png")

if __name__ == "__main__":
    for f in (espresso, americano, cappuccino, latte, macchiato, mocha, flatwhite, raf, hotchoc):
        f(); print("done", f.__name__)
