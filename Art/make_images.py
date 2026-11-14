"""Generates placeholder art: About/Preview.png and the Reroll command icon.

Run from the repo root: python -I Art/make_images.py
Kept in Art/ (never published) so the images can be regenerated or replaced.
"""
import os
from PIL import Image, ImageDraw, ImageFont

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))


def font(size):
    for name in ("arialbd.ttf", "arial.ttf", "DejaVuSans-Bold.ttf"):
        try:
            return ImageFont.truetype(name, size)
        except OSError:
            pass
    return ImageFont.load_default()


def preview():
    img = Image.new("RGB", (640, 360), (28, 30, 34))
    d = ImageDraw.Draw(img)
    f = font(72)
    text = "Downgrade"
    w = d.textlength(text, font=f)
    d.text(((640 - w) / 2, 120), text, font=f, fill=(225, 225, 225))
    f2 = font(24)
    sub = "Era-appropriate gear for every raid"
    w2 = d.textlength(sub, font=f2)
    d.text(((640 - w2) / 2, 220), sub, font=f2, fill=(160, 160, 160))
    img.save(os.path.join(ROOT, "About", "Preview.png"))


def icon():
    s = 256  # drawn large, scaled down to 64 for smooth edges
    img = Image.new("RGBA", (s, s), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    c = (215, 215, 215, 255)
    # sword, diagonal from top-right to centre-left
    d.line([(200, 30), (80, 150)], fill=c, width=22)
    d.line([(60, 120), (110, 170)], fill=c, width=18)  # crossguard
    d.line([(80, 150), (45, 185)], fill=c, width=16)   # grip
    # downward arrow on the right
    d.rectangle([(170, 120), (206, 190)], fill=c)
    d.polygon([(140, 185), (236, 185), (188, 240)], fill=c)
    img = img.resize((64, 64), Image.LANCZOS)
    img.save(os.path.join(ROOT, "Textures", "UI", "Commands", "Downgrade_Reroll.png"))


if __name__ == "__main__":
    preview()
    icon()
    print("ok")
