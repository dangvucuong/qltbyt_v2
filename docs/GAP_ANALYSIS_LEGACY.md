# Rà soát chức năng — QLTBYT_NEW so với WebForms cũ

**Ngày:** 2026-08-03  
**Legacy:** `D:\DVC\PROJECT\QLTBTYT\QLTBYT\QLTBYT` (~52 trang `.aspx`)  
**Mới:** `QLTBYT_NEW` (API .NET 7 + React)

---

## 1. Tóm tắt

| Mức | Đánh giá |
|-----|----------|
| Nghiệp vụ lõi (TB, sự cố, đơn vị, nhân sự, tài khoản) | **Đã có** tương đương hoặc gần đủ |
| Phase 3–4 (gói thầu, hồ sơ PL, kho TVT, ký PDF, camera) | **Đã có** bản REST/React |
| **Báo cáo / thống kê** | **Có một phần** — dashboard + Excel + số liệu tổng; **chưa** đủ màn hình lọc/biểu đồ và báo cáo in như cũ |
| Mobile / push / email thật | **Chưa** (stub Development) |
| Một số trang phụ / legacy | **Chưa** hoặc cố ý bỏ |

---

## 2. Báo cáo & thống kê — đã có gì?

### 2.1 Đã triển khai (QLTBYT_NEW)

| Vị trí | Nội dung |
|--------|----------|
| **Tổng quan** `/dashboard` | Tổng số TB; sắp hết hạn kiểm định (90 ngày); biểu đồ **theo trạng thái** (pie); **theo loại TB** (bar). API: `GET /api/dashboard/summary` |
| **Báo cáo** `/reports` | Thẻ số liệu: tổng TB, sắp hết hạn KĐ (30 ngày), cần bảo trì, đang hoạt động. **Xuất Excel:** danh sách TB; tài sản TB; TB sắp hết hạn KĐ (90 ngày). **In** (print CSS). API: `GET /api/reports/devices/summary`, `.../export`, `.../assets/export`, `.../expiring/export` |
| **Kho TVT** `/warehouse` | Thẻ: số mặt hàng, tồn kho, dòng nhập/xuất. API: `GET /api/warehouse/report` |
| **Test tự động** | Phase 3: summary + export; Phase 4: kho |

**Kết luận:** Hệ thống **đã có báo cáo thống kê cơ bản** (dashboard + báo cáo số liệu + Excel). Chưa thay hoàn toàn các màn **baocao.aspx** / **Dashboard.aspx** cũ (chi tiết bên dưới).

### 2.2 So với legacy — phần báo cáo còn thiếu / yếu

| Legacy | Chức năng cũ | QLTBYT_NEW | Ghi chú |
|--------|--------------|------------|---------|
| `Dashboard.aspx` | Biểu đồ **theo khoa/phòng**: số TB + **tổng giá trị** (Thanhtien, triệu đồng) | Chỉ theo **trạng thái/loại**; **không** theo đơn vị, **không** giá trị tài sản | Nên bổ sung API + chart theo `OrganizationId` |
| `baocao.aspx` | Lưới DevExpress lọc/sort nhiều cột (nguyên giá, khấu hao, nguồn vốn, đơn vị BT…) + xuất Excel | Chỉ **xuất Excel** đơn giản + vài số trên `/reports`; **không** lưới báo cáo trên web | Cần trang báo cáo lọc hoặc mở rộng export |
| `baocaoTS.aspx` | Báo cáo **tài sản** trên lưới + Excel | **Có** `assets/export` | Thiếu lưới xem trước trên UI |
| `baocaokho.aspx` | Báo cáo kho **theo khoảng ngày** (nhập, tồn, tổng kết) + xuất | Kho CRUD + số liệu tổng; **không** báo cáo theo ngày, **không** Excel kho | `GET /api/reports/warehouse` (cursorrules) **chưa** có trong `ReportsController` |
| `viewrp.aspx` + `Reports/` | In phiếu DevExpress (.repx): phiếu sửa chữa, kiểm tra… | **Không** DevExpress Report; ký PDF iText; in báo cáo tổng hợp đơn giản | Phase 4.4 một phần |
| `prBCKT.vb` | Mẫu báo cáo kiểm tra TB | Chưa | |
| `loga.aspx` | Xuất Excel **sự cố / log** | `/events` không xuất Excel | |
| Bảo trì `MocTGBT` | Nhóm TB đến hạn bảo trì | `maintenanceDueCount` parse `MocTGBT` + lưới báo cáo cột bảo trì | |

---

## 3. Ma trận trang `.aspx` → trạng thái

| Trang cũ | Trạng thái NEW | Route / API |
|----------|----------------|-------------|
| `login.aspx` | ✅ | `/login`, `POST /api/auth/login` |
| `Dashboard.aspx` | ⚠️ Một phần | `/dashboard` — thiếu thống kê theo đơn vị + giá trị |
| `Timkiem.aspx` | ✅ | `/devices` |
| `Themdulieu.aspx`, `edit.aspx`, `view.aspx` | ✅ | `/devices/new`, `/:id/edit`, `/:id` |
| `import.aspx`, `importdata.aspx` | ✅ | Import trên danh sách TB |
| `guiyc.aspx` | ✅ | Modal báo sự cố / `POST .../events` |
| `xuly.aspx`, `log.aspx`, `loga.aspx` | ⚠️ | `/events` — **không** export Excel như `loga` |
| `taikhoan.aspx` | ✅ | `/users` |
| `doimatkhau.aspx` | ⚠️ API | `POST /api/auth/change-password` — **chưa** trang Profile |
| `quenmatkhau.aspx` | ⚠️ API | `POST /api/auth/forgot-password` — **chưa** UI |
| `quanlynhansu.aspx` | ✅ | `/personnel` |
| `khoaphong.aspx`, `donvi.aspx` | ✅ | `/organizations` |
| `goithau.aspx`, `chinhsuagoithau.aspx`, `themmoigoithau.aspx` | ✅ | `/bidding` |
| `hoso.aspx`, `thongtinhs.aspx` | ✅ | `/legal-documents`, tab TB |
| `baocao.aspx` | ⚠️ | Excel + summary — **không** lưới báo cáo đầy đủ |
| `baocaoTS.aspx` | ⚠️ | `assets/export` |
| `baocaokho.aspx` | ⚠️ | `/warehouse` — thiếu báo cáo theo ngày + export |
| `Nhaphanghoa.aspx`, `Xuathanghoa.aspx`, `vattu.aspx` | ⚠️ | `/warehouse` — không tách trang nhập/xuất riêng như cũ |
| `chuyengiao.aspx` | ✅ | Chuyển giao trên chi tiết TB |
| `history.aspx`, `lichsu.aspx` | ✅ | Tab Lịch sử |
| `scanqr.aspx`, `testqr.aspx` | ⚠️ | Tab QR — **không** quét camera QR |
| `sign.aspx` | ✅ | `/sign-document`, ký PDF |
| `Camera.aspx` | ✅ | Tab Chụp ảnh |
| `CaptureNhatky.aspx` | ❌ Cố ý | Deprecated — dùng `/usage-logs` |
| `SudungTB.aspx`, `NhatkyTB.aspx` | ⚠️ | `/usage-logs` — không form “theo ngày” trên TB như cũ |
| `thongbao.aspx` | ⚠️ | Chuông header — **không** trang danh sách đầy đủ |
| `log.aspx` (hệ thống) | ⚠️ | Audit DB — **không** UI `loghethong` |
| `qltbi.aspx` | ✅ Gộp | Quản lý TB = `/devices` |
| `viewrp.aspx` | ❌ | DevExpress in phiếu — chưa |
| `upload.aspx`, `test.aspx`, `Default.aspx` | — | Không cần / dev |
| `location.aspx` | ❓ | Chưa rõ map — kiểm tra nếu còn dùng |

---

## 4. API / WebMethod còn thiếu (chọn lọc)

Tham chiếu `docs/INVENTORY_WEBMETHOD.md` (~120 method). Đã map phần lớn REST theo Phase 1–4. **Chưa / khác:**

- `UpdateRegID`, push iOS/Android đầy đủ (stub dev)
- `SendMail` production SMTP
- `LayIDfolderLienquan` / folder TB liên quan
- `InsertEveryday` / `CaptureNhatky` (legacy nhật ký ảnh)
- CRUD danh mục **countries / event-keywords / task-types** trên UI admin (API `GET /api/master/*` có, **MasterDataPage** chỉ loại TB + trạng thái)
- `GET /api/reports/warehouse` (xuất báo cáo kho) — chưa implement
- Một số field `Capnhatthietbi_admin` / `user` (nguồn vốn, vị trí, khấu hao…) — form TB mới **rút gọn**

---

## 5. Đề xuất ưu tiên (nếu cần sát legacy)

**Trạng thái (đã triển khai):**

1. **Báo cáo:** Dashboard `byOrganization` (số TB + giá trị `Thanhtien`); `/reports` tab **Lưới báo cáo** (`GET /api/reports/devices/grid`); tab **Kho TVT** xuất `GET /api/reports/warehouse/export`; export Excel sự cố.
2. **Bảo trì:** `maintenanceDueCount` theo `MocTGBT` (parse ngày legacy).
3. **UI:** `/profile` đổi MK; `/forgot-password` + `/reset-password`; `/notifications`; `/scan-qr`.
4. **In:** HTML phiếu sửa chữa / kiểm định (`GET /api/reports/devices/{id}/print/repair|inspection`) — nút trên chi tiết TB.

**Còn mở (ngoài phạm vi batch này):** DevExpress `.repx` đầy đủ; một số field form TB legacy; CRUD master countries/event-keywords trên UI.

---

## 6. Liên kết test

- Tổng hợp: `docs/HUONG_DAN_TEST_TONG_HOP.md`
- Báo cáo hiện có: mục **Báo cáo** và **Tổng quan** trong tài liệu trên; API `ReportsController`, `DashboardController`, `WarehouseController.report`.