# -*- coding: utf-8 -*-
from pathlib import Path

files = [
    Path(r"d:\DVC\PROJECT\QLTBYT_NEW\QLTBYT.API\Services\ReportService.cs"),
    Path(r"d:\DVC\PROJECT\QLTBYT_NEW\QLTBYT.API\Services\ReportService.Baocao.cs"),
    Path(r"d:\DVC\PROJECT\QLTBYT_NEW\QLTBYT.API\Services\ReportService.Kiemke.cs"),
    Path(r"d:\DVC\PROJECT\QLTBYT_NEW\QLTBYT.API\Services\WarehouseService.cs"),
    Path(r"d:\DVC\PROJECT\QLTBYT_NEW\QLTBYT.API\Middleware\ErrorHandlingMiddleware.cs"),
]

repl = {
    '"Ma TB"': '"Mã TB"',
    '"Ten TB"': '"Tên TB"',
    '"Loai"': '"Loại"',
    '"Han kiem dinh"': '"Hạn kiểm định"',
    '"Thanh tien"': '"Thành tiền"',
    '"Moc bao tri"': '"Mốc bảo trì"',
    '"Sap het han KD"': '"Sắp hết hạn KD"',
    '"Tai san TB"': '"Tài sản TB"',
    '"Ngay tiep nhan"': '"Ngày tiếp nhận"',
    '"Gia co VAT (HS)"': '"Giá có VAT (HS)"',
    '"SL nhap"': '"SL nhập"',
    '"Ton"': '"Tồn"',
    '"Ngay"': '"Ngày"',
    '"Bao cao tai san"': '"Báo cáo tài sản"',
    '"Hang SX"': '"Hãng SX"',
    '"Ky hieu"': '"Ký hiệu"',
    '"Nhom loc"': '"Nhóm lọc"',
    '"Khau hao"': '"Khấu hao"',
    '"So luong"': '"Số lượng"',
    '"Ma dinh danh"': '"Mã định danh"',
    '"Bao cao chuyen mon"': '"Báo cáo chuyên môn"',
    '"Bat dau hong"': '"Bắt đầu hỏng"',
    '"Sua xong"': '"Sửa xong"',
    '"Ghi chu"': '"Ghi chú"',
    '"So gio"': '"Số giờ"',
    '"So ca"': '"Số ca"',
    '"Ma tai san"': '"Mã tài sản"',
    "Loi he thong. Vui long thu lai.": "Lỗi hệ thống. Vui lòng thử lại.",
}

for p in files:
    t = p.read_text(encoding="utf-8")
    o = t
    for a, b in repl.items():
        t = t.replace(a, b)
    if t != o:
        p.write_text(t, encoding="utf-8", newline="\n")
        print("updated", p.name)
    else:
        print("nochange", p.name)
