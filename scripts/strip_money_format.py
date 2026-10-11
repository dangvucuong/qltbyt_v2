from pathlib import Path
import re
p = Path(r"d:\DVC\PROJECT\QLTBYT_NEW\qltbyt-frontend\src\pages\devices\DeviceFormPage.tsx")
t = p.read_text(encoding="utf-8")
t2, n = re.subn(
    r"<InputNumber\n                  min=\{0\}\n                  style=\{\{ width: '100%' \}\}\n                  formatter=\{[^\}]+(?:\{[^\}]*\}[^\}]*)*\}\n                  parser=\{[^\}]+(?:\{[^\}]*\}[^\}]*)*\}\n                />",
    "<InputNumber min={0} style={{ width: '100%' }} />",
    t,
)
print("regex n", n)
if n == 0:
    # line by line strip formatter/parser
    lines = t.splitlines()
    out = []
    skip_keys = ("formatter=", "parser=")
    for line in lines:
        if any(k in line for k in skip_keys):
            continue
        out.append(line)
    t2 = "\n".join(out) + "\n"
    print("stripped formatter/parser lines")
p.write_text(t2, encoding="utf-8", newline="\n")
print("ok")
