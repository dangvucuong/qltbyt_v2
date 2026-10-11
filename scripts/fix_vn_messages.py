# -*- coding: utf-8 -*-
"""Replace unaccented / mojibake Vietnamese user-facing messages with proper diacritics."""
from __future__ import annotations

from pathlib import Path

ROOT = Path(r"d:\DVC\PROJECT\QLTBYT_NEW\QLTBYT.API")
SKIP = {"bin", "obj"}

# Exact string replacements (longest first to avoid partial issues where needed)
REPLACEMENTS: list[tuple[str, str]] = [
    # Mojibake in DevicesController
    (
        "ChÆ°a cÃ³ há»“ sÆ¡ phÃ¡p lÃ½ cho thiáº¿t bá»‹ nÃ y.",
        "Chưa có hồ sơ pháp lý cho thiết bị này.",
    ),
    (
        "KhÃ´ng liÃªn káº¿t Ä‘Æ°á»£c há»“ sÆ¡ / thiáº¿t bá»‹.",
        "Không liên kết được hồ sơ / thiết bị.",
    ),
    (
        "LiÃªn káº¿t há»“ sÆ¡ phÃ¡p lÃ½ thÃ nh cÃ´ng.",
        "Liên kết hồ sơ pháp lý thành công.",
    ),
    # Auth
    ("Sai ten dang nhap hoac mat khau.", "Sai tên đăng nhập hoặc mật khẩu."),
    ("Dang nhap thanh cong.", "Đăng nhập thành công."),
    ("Khong hop le.", "Không hợp lệ."),
    ("Khong tim thay tai khoan.", "Không tìm thấy tài khoản."),
    ("Mat khau cu khong dung.", "Mật khẩu cũ không đúng."),
    ("Doi mat khau thanh cong.", "Đổi mật khẩu thành công."),
    ("Ma dat lai khong hop le hoac da het han.", "Mã đặt lại không hợp lệ hoặc đã hết hạn."),
    ("Dat lai mat khau thanh cong.", "Đặt lại mật khẩu thành công."),
    (
        "Neu tai khoan ton tai, ma dat lai mat khau da duoc tao (Development: xem ResetToken).",
        "Nếu tài khoản tồn tại, mã đặt lại mật khẩu đã được tạo (Development: xem ResetToken).",
    ),
    # Devices
    ("Khong tim thay thiet bi.", "Không tìm thấy thiết bị."),
    ("Them thiet bi thanh cong.", "Thêm thiết bị thành công."),
    ("Cap nhat thiet bi thanh cong.", "Cập nhật thiết bị thành công."),
    ("Xoa thiet bi thanh cong.", "Xóa thiết bị thành công."),
    ("File khong hop le.", "File không hợp lệ."),
    ("Upload file thanh cong.", "Upload file thành công."),
    ("Anh chup tu camera", "Ảnh chụp từ camera"),
    ("Luu anh chup thanh cong.", "Lưu ảnh chụp thành công."),
    ("Khong tim thay file.", "Không tìm thấy file."),
    ("Ky so PDF thanh cong.", "Ký số PDF thành công."),
    ("Chuyen giao thanh cong.", "Chuyển giao thành công."),
    ("Cap nhat ma QR/NFC thanh cong.", "Cập nhật mã QR/NFC thành công."),
    ("Khong tim thay vat tu.", "Không tìm thấy vật tư."),
    ("Import {count} thiet bi thanh cong.", "Import {count} thiết bị thành công."),
    ('$"Import {count} thiet bi thanh cong."', '$"Import {count} thiết bị thành công."'),
    ("Chuyen giao thiet bi", "Chuyển giao thiết bị"),
    ('$"Import Excel {count} thiet bi"', '$"Import Excel {count} thiết bị"'),
    # Users / orgs / personnel / bidding
    ("Tao tai khoan thanh cong.", "Tạo tài khoản thành công."),
    ("Cap nhat tai khoan thanh cong.", "Cập nhật tài khoản thành công."),
    ("Xoa tai khoan thanh cong.", "Xóa tài khoản thành công."),
    ("Ten dang nhap va mat khau la bat buoc.", "Tên đăng nhập và mật khẩu là bắt buộc."),
    ("Ten dang nhap da ton tai.", "Tên đăng nhập đã tồn tại."),
    ("Khong the xoa tai khoan dang dang nhap.", "Không thể xóa tài khoản đang đăng nhập."),
    ('$"Cap nhat tai khoan ID={id}"', '$"Cập nhật tài khoản ID={id}"'),
    ('$"Xoa tai khoan ID={id}"', '$"Xóa tài khoản ID={id}"'),
    ("Khong tim thay don vi.", "Không tìm thấy đơn vị."),
    ("Them don vi thanh cong.", "Thêm đơn vị thành công."),
    ("Cap nhat don vi thanh cong.", "Cập nhật đơn vị thành công."),
    ("Xoa don vi thanh cong.", "Xóa đơn vị thành công."),
    ("Ten don vi la bat buoc.", "Tên đơn vị là bắt buộc."),
    ("Khong xoa don vi con thiet bi.", "Không xóa đơn vị còn thiết bị."),
    ("Khong tim thay nhan su.", "Không tìm thấy nhân sự."),
    ("Them nhan su thanh cong.", "Thêm nhân sự thành công."),
    ("Cap nhat nhan su thanh cong.", "Cập nhật nhân sự thành công."),
    ("Xoa nhan su thanh cong.", "Xóa nhân sự thành công."),
    ("Ho ten la bat buoc.", "Họ tên là bắt buộc."),
    ("Khong co quyen voi don vi nay.", "Không có quyền với đơn vị này."),
    ('$"Them nhan su {entity.FullName}"', '$"Thêm nhân sự {entity.FullName}"'),
    ('$"Cap nhat nhan su ID={id}"', '$"Cập nhật nhân sự ID={id}"'),
    ('$"Xoa nhan su ID={id}"', '$"Xóa nhân sự ID={id}"'),
    ("Khong tim thay goi thau.", "Không tìm thấy gói thầu."),
    ("Them goi thau thanh cong.", "Thêm gói thầu thành công."),
    ("Cap nhat goi thau thanh cong.", "Cập nhật gói thầu thành công."),
    ("Xoa goi thau thanh cong.", "Xóa gói thầu thành công."),
    # Notifications / events / usage
    ("Da danh dau da doc.", "Đã đánh dấu đã đọc."),
    ("Da danh dau tat ca da doc.", "Đã đánh dấu tất cả đã đọc."),
    ("Them keyword thanh cong.", "Thêm từ khóa thành công."),
    ("Keyword la bat buoc.", "Từ khóa là bắt buộc."),
    ("Ghi nhat ky thanh cong.", "Ghi nhật ký thành công."),
    ("Khong co quyen voi thiet bi nay.", "Không có quyền với thiết bị này."),
    # Warehouse
    ("Them hang hoa thanh cong.", "Thêm hàng hóa thành công."),
    ("Them lo hang thanh cong.", "Thêm lô hàng thành công."),
    ("Nhap kho thanh cong.", "Nhập kho thành công."),
    ("Xuat kho thanh cong.", "Xuất kho thành công."),
    ("So luong nhap phai lon hon 0.", "Số lượng nhập phải lớn hơn 0."),
    ("So luong xuat phai lon hon 0.", "Số lượng xuất phải lớn hơn 0."),
    ("Khong tim thay dong nhap kho.", "Không tìm thấy dòng nhập kho."),
    ("So luong xuat vuot ton kho.", "Số lượng xuất vượt tồn kho."),
    # Admin / master
    ("Khong tim thay loai TB.", "Không tìm thấy loại TB."),
    ("Khong tim thay trang thai.", "Không tìm thấy trạng thái."),
    ("Ten loai TB la bat buoc.", "Tên loại TB là bắt buộc."),
    ("Ten trang thai la bat buoc.", "Tên trạng thái là bắt buộc."),
    # Health / validation
    ("Database chua san sang hoac thieu bang core.", "Database chưa sẵn sàng hoặc thiếu bảng core."),
    ("Database san sang.", "Database sẵn sàng."),
    ("Du lieu khong hop le.", "Dữ liệu không hợp lệ."),
    # Validators
    ("Ten dang nhap khong duoc de trống.", "Tên đăng nhập không được để trống."),
    ("Mat khau khong duoc de trống.", "Mật khẩu không được để trống."),
    ("Mat khau phai co it nhat 6 ky tu.", "Mật khẩu phải có ít nhất 6 ký tự."),
    ("Mat khau cu khong duoc de trống.", "Mật khẩu cũ không được để trống."),
    ("Mat khau moi khong duoc de trống.", "Mật khẩu mới không được để trống."),
    ("Mat khau moi phai co it nhat 6 ky tu.", "Mật khẩu mới phải có ít nhất 6 ký tự."),
    ("Mat khau moi phai khac mat khau cu.", "Mật khẩu mới phải khác mật khẩu cũ."),
    ("Ma dat lai khong hop le.", "Mã đặt lại không hợp lệ."),
    ("Can nhap email hoac ten dang nhap.", "Cần nhập email hoặc tên đăng nhập."),
    # PDF / print / alerts / job
    ("Chi ho tro ky so file PDF.", "Chỉ hỗ trợ ký số file PDF."),
    ('$"Da ky so: {signerName} - {DateTime.Now:dd/MM/yyyy HH:mm}"',
     '$"Đã ký số: {signerName} - {DateTime.Now:dd/MM/yyyy HH:mm}"'),
    ('" [Ky so]"', '" [Ký số]"'),
    ('User.Identity?.Name ?? "Ky thuat"', 'User.Identity?.Name ?? "Kỹ thuật"'),
    ("Thiet bi sap het han kiem dinh", "Thiết bị sắp hết hạn kiểm định"),
    ('$"[QLTBYT] Su co thiet bi {deviceCode}"', '$"[QLTBYT] Sự cố thiết bị {deviceCode}"'),
    ('$"[QLTBYT] Da xu ly su co {deviceCode}"', '$"[QLTBYT] Đã xử lý sự cố {deviceCode}"'),
    (
        '$"Thiet bi: {deviceCode} (ID={deviceId})\\nNoi dung: {content}\\nThoi gian: {DateTime.Now:dd/MM/yyyy HH:mm}"',
        '$"Thiết bị: {deviceCode} (ID={deviceId})\\nNội dung: {content}\\nThời gian: {DateTime.Now:dd/MM/yyyy HH:mm}"',
    ),
    ('"Yeu cau thiet bi moi"', '"Yêu cầu thiết bị mới"'),
    (
        '$"Su co ID={eventId}, thiet bi {deviceCode} da duoc xu ly.\\nThoi gian: {DateTime.Now:dd/MM/yyyy HH:mm}"',
        '$"Sự cố ID={eventId}, thiết bị {deviceCode} đã được xử lý.\\nThời gian: {DateTime.Now:dd/MM/yyyy HH:mm}"',
    ),
    ('"Su co da xu ly"', '"Sự cố đã xử lý"'),
    ('$"Thiet bi {deviceCode}"', '$"Thiết bị {deviceCode}"'),
    # Partial fragments that appear in interpolated / concatenated messages
    ("thiet bi thanh cong.", "thiết bị thành công."),
    ("Import Excel {count} thiet bi", "Import Excel {count} thiết bị"),
    # Dashboard labels
    ('"Khong ro"', '"Không rõ"'),
    # Legacy usage journal
    (
        "Nhat ky text/anh legacy (NhatkyTB.aspx, CaptureNhatky) khong duoc ho tro tren API moi.",
        "Nhật ký text/ảnh legacy (NhatkyTB.aspx, CaptureNhatky) không được hỗ trợ trên API mới.",
    ),
    (
        "Chuc nang chup anh nhat ky legacy da deprecated. Su dung POST /api/usage-logs hoac bao su co thiet bi.",
        "Chức năng chụp ảnh nhật ký legacy đã deprecated. Sử dụng POST /api/usage-logs hoặc báo sự cố thiết bị.",
    ),
    # Print templates (user-facing HTML)
    ("PHIEU YEU CAU / SUA CHUA THIET BI", "PHIẾU YÊU CẦU / SỬA CHỮA THIẾT BỊ"),
    ("In luc:", "In lúc:"),
    ("Ky thuat vien:", "Kỹ thuật viên:"),
    ("Don vi nhan:", "Đơn vị nhận:"),
    ("BIEN BAN KIEM TRA / KIEM DINH THIET BI", "BIÊN BẢN KIỂM TRA / KIỂM ĐỊNH THIẾT BỊ"),
    # Report sheet name
    ('Worksheets.Add("Thiet bi")', 'Worksheets.Add("Thiết bị")'),
]

# Sort by old length desc so longer phrases win first
REPLACEMENTS.sort(key=lambda x: len(x[0]), reverse=True)


def iter_cs_files() -> list[Path]:
    files: list[Path] = []
    for p in ROOT.rglob("*.cs"):
        if any(part in SKIP for part in p.parts):
            continue
        files.append(p)
    return files


def main() -> None:
    changed_files = 0
    total_hits = 0
    for path in iter_cs_files():
        text = path.read_text(encoding="utf-8")
        original = text
        file_hits = 0
        for old, new in REPLACEMENTS:
            if old in text:
                count = text.count(old)
                text = text.replace(old, new)
                file_hits += count
        if text != original:
            path.write_text(text, encoding="utf-8", newline="\n")
            changed_files += 1
            total_hits += file_hits
            print(f"  {path.relative_to(ROOT)} ({file_hits})")
    print(f"Done: {changed_files} files, {total_hits} replacements")


if __name__ == "__main__":
    main()
