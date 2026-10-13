# Mẫu báo cáo DevExpress (tham chiếu)

Các PDF xuất từ report cũ (`.repx`) dùng làm chuẩn layout cho HTML in trên hệ mới:

| File | Report cũ | viewrp type | API mới |
|---|---|---|---|
| `rpPhieusuachua.pdf` | `rpPhieusuachua` | 0 | `/api/reports/devices/{id}/print/repair` |
| `prBCKT.pdf` | `prBCKT` | 1 | `/api/reports/devices/{id}/print/handover` |
| `rpPhieuNT.pdf` | `rpPhieuNT` | 2 | `/api/reports/devices/{id}/print/acceptance` |

Header bệnh viện cấu hình tại `PrintSettings` trong `appsettings.json`.
