from PIL import Image
import os

src = r"d:\DVC\PROJECT\QLTBYT_NEW\update\dvc-techflow-edit.png"
dst = r"d:\DVC\PROJECT\QLTBYT_NEW\qltbyt-frontend\public\brand\dvc-techflow-logo.png"
dst_mark = r"d:\DVC\PROJECT\QLTBYT_NEW\qltbyt-frontend\public\brand\dvc-techflow-mark.png"

img = Image.open(src).convert("RGBA")
pixels = img.load()
w, h = img.size
for y in range(h):
    for x in range(w):
        r, g, b, a = pixels[x, y]
        # Near-black / dark gray background -> transparent
        if r < 35 and g < 35 and b < 35:
            pixels[x, y] = (0, 0, 0, 0)
        # Soft edge for near-black to reduce halo
        elif r < 55 and g < 55 and b < 55:
            alpha = int(255 * ((max(r, g, b) - 20) / 35))
            alpha = max(0, min(255, alpha))
            pixels[x, y] = (r, g, b, alpha)

# Crop tight around non-transparent content
bbox = img.getbbox()
if bbox:
    img = img.crop(bbox)

# Pad a little for breathing room
pad = 12
canvas = Image.new("RGBA", (img.width + pad * 2, img.height + pad * 2), (0, 0, 0, 0))
canvas.paste(img, (pad, pad), img)
canvas.save(dst, optimize=True)

# Mark-only square crop for sidebar: prefer upper graphic (top ~62%)
mh = int(canvas.height * 0.62)
mark = canvas.crop((0, 0, canvas.width, mh))
side = max(mark.width, mark.height)
sq = Image.new("RGBA", (side, side), (0, 0, 0, 0))
sq.paste(mark, ((side - mark.width) // 2, (side - mark.height) // 2), mark)
sq = sq.resize((128, 128), Image.Resampling.LANCZOS)
sq.save(dst_mark, optimize=True)
print("saved", dst, os.path.getsize(dst), "mark", os.path.getsize(dst_mark), "size", canvas.size)
