# -*- coding: utf-8 -*-
from pathlib import Path

ROOT = Path(r"d:\DVC\PROJECT\QLTBYT_NEW\QLTBYT.API")
SKIP = {"bin", "obj"}

REPLACEMENTS = [
    ('throw new InvalidOperationException("Ma goi thau la bat buoc.");',
     'throw new InvalidOperationException("Mã gói thầu là bắt buộc.");'),
    ('Keyword = "Chuyen giao"', 'Keyword = "Chuyển giao"'),
    ('$"Chuyen giao TB {device.DeviceCode}"', '$"Chuyển giao TB {device.DeviceCode}"'),
    ('Type = "Su co / yeu cau"', 'Type = "Sự cố / yêu cầu"'),
    ('Type = "Nhat ky su dung"', 'Type = "Nhật ký sử dụng"'),
    ('$"Nhat ky su dung {storageKey}"', '$"Nhật ký sử dụng {storageKey}"'),
    ('$"Tao tai khoan {entity.Username}"', '$"Tạo tài khoản {entity.Username}"'),
    # Print templates
    ("<th>Ma TB</th>", "<th>Mã TB</th>"),
    ("<th>Don vi</th>", "<th>Đơn vị</th>"),
    ("Noi dung yeu cau / sua chua:", "Nội dung yêu cầu / sửa chữa:"),
    # Excel headers / sheet names
    ('sheet.Cell(1, 5).Value = "Don vi";', 'sheet.Cell(1, 5).Value = "Đơn vị";'),
    ('sheet.Cell(1, 4).Value = "Don vi";', 'sheet.Cell(1, 4).Value = "Đơn vị";'),
    ('sheet.Cell(1, 11).Value = "Don vi tinh";', 'sheet.Cell(1, 11).Value = "Đơn vị tính";'),
    ('sheet.Cell(1, 5).Value = "Trang thai";', 'sheet.Cell(1, 5).Value = "Trạng thái";'),
    ('sheet.Cell(1, 6).Value = "Goi thau";', 'sheet.Cell(1, 6).Value = "Gói thầu";'),
    ('Worksheets.Add("Su co")', 'Worksheets.Add("Sự cố")'),
    ('sheet.Cell(1, 3).Value = "Noi dung";', 'sheet.Cell(1, 3).Value = "Nội dung";'),
    ('sheet.Cell(1, 4).Value = "Thoi gian";', 'sheet.Cell(1, 4).Value = "Thời gian";'),
    ('Worksheets.Add("Nhap kho")', 'Worksheets.Add("Nhập kho")'),
    ('sheet.Cell(1, 2).Value = "Hang hoa";', 'sheet.Cell(1, 2).Value = "Hàng hóa";'),
    # Sample demo content (displayable)
    (
        'Content = "May do huyet ap khong hoat dong - yeu cau kiem tra demo"',
        'Content = "Máy đo huyết áp không hoạt động - yêu cầu kiểm tra demo"',
    ),
    (
        'Name = "Kim tiem vat tu y te (demo)"',
        'Name = "Kim tiêm vật tư y tế (demo)"',
    ),
]

changed = 0
hits = 0
for p in ROOT.rglob("*.cs"):
    if any(x in p.parts for x in SKIP):
        continue
    text = p.read_text(encoding="utf-8")
    orig = text
    n = 0
    for old, new in REPLACEMENTS:
        if old in text:
            c = text.count(old)
            text = text.replace(old, new)
            n += c
    if text != orig:
        p.write_text(text, encoding="utf-8", newline="\n")
        changed += 1
        hits += n
        print(f"  {p.relative_to(ROOT)} ({n})")
print(f"Done: {changed} files, {hits} replacements")
