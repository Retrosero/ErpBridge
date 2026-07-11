"""
Generate a modern ErpBridge app icon.

Design:
- Rounded square with a blue->teal linear gradient.
- White "E" letter on the left (ErpBridge monogram) plus a small
  chain-link glyph on the right (the "bridge" between ERP and the
  central API).
- Soft inner highlight + subtle drop shadow for a flat-modern feel.
- Output:
    - assets/icon-256.png   (master PNG used by the WPF / NotifyIcon)
    - assets/icon.ico       (256-frame ICO — Windows scales as needed)
    - assets/tray-32.png    (tray variant, 32x32 with a green status dot)
"""

from __future__ import annotations

import math
import os
from PIL import Image, ImageDraw, ImageFilter, ImageFont

OUT_DIR = os.path.join(os.path.dirname(os.path.abspath(__file__)))
os.makedirs(OUT_DIR, exist_ok=True)


def _interp(c1, c2, t):
    return tuple(int(c1[i] + (c2[i] - c1[i]) * t) for i in range(3))


def _make_canvas(size: int) -> Image.Image:
    """Render the icon at the given size. Returns an RGBA PIL image."""
    scale = 4  # supersample 4x for crisp edges
    s = size * scale
    img = Image.new("RGBA", (s, s), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    # Rounded square clip path
    radius = int(s * 0.22)
    pad = int(s * 0.04)
    bbox = (pad, pad, s - pad, s - pad)

    # Gradient: top-left #1E40AF (indigo-800) -> bottom-right #14B8A6 (teal-500)
    top = (0x1E, 0x40, 0xAF)
    bot = (0x14, 0xB8, 0xA6)
    for y in range(s):
        t = y / max(1, s - 1)
        line_color = _interp(top, bot, t) + (255,)
        draw.line([(0, y), (s, y)], fill=line_color)

    gradient = img.copy()

    # Mask: rounded square
    mask = Image.new("L", (s, s), 0)
    mdraw = ImageDraw.Draw(mask)
    mdraw.rounded_rectangle(bbox, radius=radius, fill=255)

    canvas = Image.new("RGBA", (s, s), (0, 0, 0, 0))
    canvas.paste(gradient, (0, 0), mask)

    # Soft inner highlight at the top
    highlight = Image.new("RGBA", (s, s), (0, 0, 0, 0))
    hdraw = ImageDraw.Draw(highlight)
    hdraw.rounded_rectangle(bbox, radius=radius, fill=(255, 255, 255, 70))
    highlight = highlight.filter(ImageFilter.GaussianBlur(radius=s * 0.06))
    canvas = Image.alpha_composite(canvas, highlight)

    # Subtle inner shadow at the bottom for depth
    shadow = Image.new("RGBA", (s, s), (0, 0, 0, 0))
    sdraw = ImageDraw.Draw(shadow)
    sdraw.rounded_rectangle(bbox, radius=radius, fill=(0, 0, 0, 70))
    shadow = shadow.filter(ImageFilter.GaussianBlur(radius=s * 0.08))
    canvas = Image.alpha_composite(canvas, shadow)

    draw = ImageDraw.Draw(canvas)

    # "E" monogram (left side)
    try:
        font_path = "C:/Windows/Fonts/segoeuib.ttf"
        e_font = ImageFont.truetype(font_path, int(s * 0.55))
    except OSError:
        e_font = ImageFont.load_default()
    e_text = "E"
    ebbox = draw.textbbox((0, 0), e_text, font=e_font)
    eh = ebbox[3] - ebbox[1]
    e_x = int(s * 0.18) - ebbox[0]
    e_y = (s - eh) // 2 - ebbox[1]
    for dx, dy in ((-1, 0), (1, 0), (0, -1), (0, 1)):
        draw.text((e_x + dx, e_y + dy), e_text, font=e_font, fill=(15, 23, 42, 120))
    draw.text((e_x, e_y), e_text, font=e_font, fill=(255, 255, 255, 255))

    # Chain-link glyph (right side)
    link_color = (255, 255, 255, 235)
    cx, cy = int(s * 0.70), int(s * 0.50)
    rx, ry = int(s * 0.07), int(s * 0.035)
    rot = int(s * 0.16)
    thickness = int(s * 0.045)

    def _rotate(pts, angle_deg, ox, oy):
        a = math.radians(angle_deg)
        ca, sa = math.cos(a), math.sin(a)
        out = []
        for x, y in pts:
            dx, dy = x - ox, y - oy
            out.append((ox + dx * ca - dy * sa, oy + dx * sa + dy * ca))
        return out

    back = _rotate(
        [
            (cx - rot - rx, cy - ry),
            (cx - rot + rx, cy - ry),
            (cx - rot + rx, cy + ry),
            (cx - rot - rx, cy + ry),
        ],
        -25,
        cx,
        cy,
    )
    front = _rotate(
        [
            (cx + rot - rx, cy - ry),
            (cx + rot + rx, cy - ry),
            (cx + rot + rx, cy + ry),
            (cx + rot - rx, cy + ry),
        ],
        -25,
        cx,
        cy,
    )

    def _draw_link_outline(d, pts, color, width):
        for i in range(len(pts)):
            x1, y1 = pts[i]
            x2, y2 = pts[(i + 1) % len(pts)]
            d.line([(x1, y1), (x2, y2)], fill=color, width=width)
        for x, y in pts:
            d.ellipse(
                (x - width / 2, y - width / 2, x + width / 2, y + width / 2),
                fill=color,
            )

    _draw_link_outline(draw, back, link_color, thickness)
    _draw_link_outline(draw, front, link_color, thickness)

    return canvas.resize((size, size), Image.LANCZOS)


def main() -> None:
    # 1) Master 256x256 PNG (used by ApplicationIcon + TaskbarIcon)
    master = _make_canvas(256)
    master_path = os.path.join(OUT_DIR, "icon-256.png")
    master.save(master_path, format="PNG")
    print(f"wrote {master_path}")

    # 2) ICO — PIL ICO writer with a single 256 frame. Windows scales it
    # down to 16/32/48 at display time. Good enough for the system tray
    # and the taskbar; for an aggressive multi-resolution icon use
    # ImageMagick's `convert` — out of scope here.
    ico_path = os.path.join(OUT_DIR, "icon.ico")
    master.save(ico_path, format="ICO", sizes=[(256, 256)])
    print(f"wrote {ico_path}")

    # 3) Tray icon — 32x32 with a small green dot for "running"
    tray = _make_canvas(32)
    tdraw = ImageDraw.Draw(tray)
    dot_r = 6
    dot_cx, dot_cy = 32 - 7, 32 - 7
    tdraw.ellipse(
        (dot_cx - dot_r - 1, dot_cy - dot_r - 1, dot_cx + dot_r + 1, dot_cy + dot_r + 1),
        fill=(15, 23, 42, 200),
    )
    tdraw.ellipse(
        (dot_cx - dot_r, dot_cy - dot_r, dot_cx + dot_r, dot_cy + dot_r),
        fill=(34, 197, 94, 255),
    )
    tray_path = os.path.join(OUT_DIR, "tray-32.png")
    tray.save(tray_path, format="PNG")
    print(f"wrote {tray_path}")


if __name__ == "__main__":
    main()
