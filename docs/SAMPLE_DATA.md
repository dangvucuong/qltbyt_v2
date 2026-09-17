# Dữ liệu mẫu (Development)

`DevelopmentDataSeeder` + `SampleDataVietnameseRefresher`: chuẩn hóa tiếng Việt có dấu và tạo lại thiết bị `TB-DEMO-*` mỗi lần khởi động Development.

## Tài khoản (`Sample@123`)

| Username | Họ tên |
|----------|--------|
| admin | Quản trị QLTBYT |
| kythuat | Kỹ thuật viên |
| user | Nhân viên khoa |

## Thiết bị demo

| MaTB | Tên |
|------|-----|
| TB-DEMO-001 | Máy thợ oxy demo |
| TB-DEMO-002 | Máy đo huyết áp demo |
| TB-DEMO-003 | Máy siêu âm demo |

## Gói thầu & hồ sơ pháp lý (Development)

| Mã / đối tượng | Nội dung |
|----------------|----------|
| GT-DEMO-2024 | Gói thầu mua máy thở oxy demo — gắn TB-DEMO-* |
| Hồ sơ TB-DEMO-001 | KD-DEMO-2024-001, NK-DEMO-2024-001, giá VAT 125.000.000 VND |

## Danh mục

Trạng thái: Đang hoạt động; Không hoạt động / Hỏng; Chưa sử dụng. Loại: Máy thở, Máy đo, Thiết bị hình ảnh. Đơn vị: Khoa Nội, Khoa Cấp cứu, Khoa Hồi sức.

Script: `python scripts/refresh_sample_data_vn.py`


## Kho TVT (Phase 4)
- Hang: VT-DEMO-001 — Kim tiem vat tu y te (demo)
- Lo: LO-DEMO-2024, ton nhap 500
