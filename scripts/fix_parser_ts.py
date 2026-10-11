from pathlib import Path
p = Path(r"d:\DVC\PROJECT\QLTBYT_NEW\qltbyt-frontend\src\pages\devices\DeviceFormPage.tsx")
t = p.read_text(encoding="utf-8")
old = "parser={(v) => Number((v ?? '').replace(/,/g, '') || '0')}"
new = "parser={(v) => (v ?? '').replace(/,/g, '') as unknown as number}"
print("count", t.count(old))
t = t.replace(old, new)
p.write_text(t, encoding="utf-8", newline="\n")
print("done")
