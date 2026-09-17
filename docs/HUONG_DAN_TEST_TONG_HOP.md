# Hướng dẫn test tổng hợp — QLTBYT_NEW

**Phiên bản:** 1.0 · Cập nhật sau Phase 4  
**Đối tượng:** QA, BA, kỹ thuật viên, dev triển khai  
**Phạm vi:** Toàn bộ chức năng đã triển khai (Phase 0 → Phase 4).

Tài liệu chi tiết theo giai đoạn (tham chiếu):

| Giai đoạn | File |
|-----------|------|
| Phase 1 & 2 | `docs/HUONG_DAN_TEST_PHASE1_PHASE2.md` |
| Phase 3 | `docs/HUONG_DAN_TEST_PHASE3.md` |
| Phase 4 | `docs/HUONG_DAN_TEST_PHASE4.md` |
| Dữ liệu demo | `docs/SAMPLE_DATA.md` |
| Phân quyền / menu | `docs/PHAN_QUYEN_TAI_KHOAN.md` |

---

## 1. Tiêu chí nghiệm thu tổng thể

| Hạng mục | Yêu cầu |
|----------|---------|
| Tự động | `dotnet test` — **38** test passed, **0** failed |
| Frontend | `npm run build` trong `qltbyt-frontend` — thành công |
| UI tiếng Việt | Menu, thông báo, nhãn form có dấu (các trang React) |
| Phân quyền | 3 role Admin / Technician / User — menu và API khớp (xem `docs/PHAN_QUYEN_TAI_KHOAN.md`) |
| Demo | Thiết bị `TB-DEMO-*`, gói thầu `GT-DEMO-2024`, kho `VT-DEMO-001` |

---

## 2. Chuẩn bị môi trường

### 2.1 Phần mềm

- Windows, **SQL Server**, **.NET 7 SDK**, **Node.js 18+**
- Database: **`db_QLTBYT_NEW`** (connection trong `QLTBYT.API/appsettings.Development.json`)

### 2.2 Chạy API

```powershell
cd D:\DVC\PROJECT\QLTBYT_NEW\QLTBYT.API
dotnet run
```

- API: http://localhost:5149  
- Swagger: http://localhost:5149/swagger  
- Development: tự seed (`DevelopmentDataSeeder` + `SampleDataVietnameseRefresher`)

### 2.3 Chạy Frontend

```powershell
cd D:\DVC\PROJECT\QLTBYT_NEW\qltbyt-frontend
npm install
npm run dev
```

- UI: http://localhost:5173  
- Proxy: `/api`, `/files`, `/captures` → API

### 2.4 Tài khoản demo

Mật khẩu seed: **`Sample@123`** (chỉ khi có user seed). DB restore legacy: dùng mật khẩu cũ; role từ `LoaiTK` + `Admin` — chi tiết `docs/PHAN_QUYEN_TAI_KHOAN.md`.

| Username | Role UI | Phạm vi |
|----------|---------|---------|
| `admin` | Admin | Toàn hệ thống |
| `kythuat` | Technician | Toàn hệ thống (không quản lý tài khoản/đơn vị) |
| `user` | User | Một đơn vị (thường Khoa Nội) |

---

## 3. Kiểm thử tự động

```powershell
cd D:\DVC\PROJECT\QLTBYT_NEW
dotnet test QLTBYT.API.Tests\QLTBYT.API.Tests.csproj
```

**Kỳ vọng:** Passed **38** (bao gồm `GapClosureIntegrationTests`).

| Nhóm | Nội dung |
|------|---------|
| Step0 | Health, schema |
| Phase1 | Auth, devices, scope đơn vị, validators |
| Phase2 | Sự cố, events |
| Phase3 | Đơn vị, gói thầu, báo cáo, import, chuyển giao, hồ sơ pháp lý |
| Phase4 | Kho TVT nhập/xuất, báo cáo kho |

Lọc theo phase:

```powershell
dotnet test --filter "Phase4"
```

---

## 4. Ma trận menu theo role (UI)

| Menu | user | kythuat | admin |
|------|:----:|:-------:|:-----:|
| Tổng quan | ✓ | ✓ | ✓ |
| Sự cố / yêu cầu | ✓ | ✓ | ✓ |
| Thiết bị | ✓ | ✓ | ✓ |
| Nhật ký sử dụng | ✓ | ✓ | ✓ |
| Quét QR | | ✓ | ✓ |
| Tài khoản / thông báo | ✓ | ✓ | ✓ |
| Gói thầu | | ✓ | ✓ |
| Hồ sơ pháp lý | | ✓ | ✓ |
| Kho TVT | | ✓ | ✓ |
| Ký số PDF | | ✓ | ✓ |
| Nhân sự | | ✓ | ✓ |
| Đơn vị | | | ✓ |
| Danh mục | | | ✓ |
| Tài khoản | | | ✓ |

---

## 5. Checklist chức năng theo phase

### 5.1 Phase 1 — Core (đăng nhập, dashboard, thiết bị)

| # | Chức năng | Cách test nhanh | Kỳ vọng |
|---|-----------|-----------------|---------|
| 1 | Đăng nhập / đăng xuất | `/login` | Sai MK → lỗi; đúng → dashboard |
| 2 | Tổng quan | `/dashboard` | Số TB, biểu đồ trạng thái/loại, **bảng/biểu đồ theo đơn vị** (số lượng + giá trị tài sản) |
| 3 | Danh sách TB | `/devices`, tìm `TB-DEMO` | ≥ 3 thiết bị demo |
| 4 | Thêm / sửa TB | `/devices/new`, edit | kythuat+; user theo scope |
| 5 | Chi tiết TB | Click mã TB | Đủ trường, TB con |
| 6 | Upload tài liệu | Tab Tài liệu | File mở qua `/files/...` |
| 7 | Scope đơn vị | `user` vs `admin` | user không thấy TB-DEMO-002 (nếu khác đơn vị) |
| 8 | Xóa TB | admin | user/kythuat không xóa (UI/API) |

Chi tiết ID test: `HUONG_DAN_TEST_PHASE1_PHASE2.md` §4.

### 5.2 Phase 2 — Vận hành

| # | Chức năng | Cách test | Kỳ vọng |
|---|-----------|-----------|---------|
| 1 | Báo sự cố | TB-DEMO-001, user | Thành công, có thông báo |
| 2 | Danh sách sự cố | `/events`, kythuat | Xử lý → Đã xử lý |
| 3 | Nhật ký sử dụng | `/usage-logs` | Ghi và xem log |
| 4 | Thông báo | Chuông header | Đọc / đánh dấu đã đọc |
| 5 | Tài khoản | `/users`, admin | CRUD tài khoản |
| 6 | Nhân sự | `/personnel`, kythuat+ | CRUD nhân sự |

Chi tiết: `HUONG_DAN_TEST_PHASE1_PHASE2.md` §5.

### 5.3 Phase 3 — Nâng cao

| # | Chức năng | Route / vị trí | Kỳ vọng |
|---|-----------|----------------|---------|
| 1 | Đơn vị | `/organizations`, admin | CRUD khoa/phòng |
| 2 | Gói thầu | `/bidding` | `GT-DEMO-2024`, gán TB |
| 3 | Hồ sơ pháp lý | `/legal-documents`, tab TB | KD/NK/LHH, upload file |
| 4 | Báo cáo | `/reports` | Tab **Tổng hợp** (Excel TB/tài sản/KĐ/sự cố); tab **Lưới báo cáo** (lọc đơn vị/loại/trạng thái); tab **Kho TVT** (chọn ngày → Excel) |
| 5 | Tài khoản | `/profile` | Đổi mật khẩu (`POST /api/auth/change-password`) |
| 6 | Quên MK | `/forgot-password` → `/reset-password` | Dev: token hiện trên trang quên MK |
| 7 | Thông báo | `/notifications` | Danh sách + đánh dấu đã đọc |
| 8 | Quét QR | `/scan-qr` | Camera (nếu trình duyệt hỗ trợ) hoặc nhập mã → mở chi tiết TB |
| 9 | In phiếu TB | `/devices/{id}` | Nút **In phiếu sửa chữa** / **In phiếu kiểm định** (HTML) |
| 5 | Import Excel TB | Danh sách TB | kythuat+ import |
| 6 | Chuyển giao | Chi tiết TB | Đổi đơn vị + lịch sử |
| 7 | Lịch sử TB | Tab Lịch sử | Có log chuyển giao / sự cố |
| 8 | Vật tư TB | Tab Vật tư | Thêm/xóa vật tư |
| 9 | QR/NFC | Tab QR | Cập nhật mã |
| 10 | Danh mục admin | `/admin/master` | Loại TB, trạng thái |

Chi tiết: `HUONG_DAN_TEST_PHASE3.md`.

### 5.4 Phase 4 — Premium

| # | Chức năng | Route / vị trí | Kỳ vọng |
|---|-----------|----------------|---------|
| 1 | Kho TVT | `/warehouse` | Hàng `VT-DEMO-001`, nhập/xuất tồn |
| 2 | Ký số PDF | Tab Tài liệu / `/sign-document` | File PDF → `_kyso.pdf` |
| 3 | Camera | Tab Chụp ảnh | Ảnh lưu `Captures/`, hiện Tài liệu |
| 4 | In báo cáo | `/reports` → In báo cáo | Print preview, ẩn menu khi in |
| 5 | Responsive | Thu nhỏ màn hình | Sidebar ẩn, padding mobile |
| 6 | Cache dashboard | API gọi lặp | Phản hồi nhanh (cache 60s) |

Chi tiết: `HUONG_DAN_TEST_PHASE4.md`, deploy: `docs/DEPLOY_IIS.md`.

---

## 6. Dữ liệu demo quan trọng

| Loại | Mã / giá trị | Ghi chú |
|------|--------------|---------|
| Thiết bị | TB-DEMO-001, 002, 003 | 003 là TB con của 001 |
| Gói thầu | GT-DEMO-2024 | Gán cho TB demo |
| Hồ sơ PL | KD-DEMO-2024-001 | Liên kết TB-DEMO-001 |
| Kho TVT | VT-DEMO-001, lô LO-DEMO-2024 | Tồn 500 |

Đầy đủ: `docs/SAMPLE_DATA.md`.

---

## 7. API tham chiếu nhanh

| Nhóm | Endpoint (mẫu) |
|------|----------------|
| Auth | `POST /api/auth/login`, `GET /api/auth/me` |
| Dashboard | `GET /api/dashboard/summary` |
| Thiết bị | `GET/POST/PUT/DELETE /api/devices`, `POST .../files`, `POST .../captures`, `POST /api/devices/files/{id}/sign-pdf` |
| Sự cố | `GET/PUT /api/events`, `POST /api/devices/{id}/events` |
| Báo cáo | `GET /api/dashboard/summary` (`byOrganization`), `GET /api/reports/devices/grid`, `GET /api/reports/devices/summary`, export Excel, `GET /api/reports/warehouse/export`, `GET /api/reports/devices/{id}/print/repair|inspection` |
| Gói thầu | `GET/POST /api/bidding` |
| Hồ sơ PL | `GET/POST /api/legal-documents` |
| Kho | `GET/POST /api/warehouse/commodities`, `imports`, `exports`, `report` |
| Admin | `GET/POST /api/users`, `GET/POST /api/organizations`, `GET/POST /api/admin/master/*` |

Swagger: http://localhost:5149/swagger

---

## 8. Kịch bản smoke end-to-end (≈ 30 phút)

1. **admin** — đăng nhập → dashboard → đơn vị → danh mục → tài khoản.  
2. **kythuat** — báo cáo (tải 1 Excel) → gói thầu → kho TVT (xuất 1) → TB-DEMO-001: upload PDF, ký số, chụp ảnh, chuyển giao (nếu cần rollback DB).  
3. **user** — chỉ thiết bị trong scope → báo sự cố → không thấy Kho / Ký số / Đơn vị.  
4. `dotnet test` → 38 passed.

---

## 9. Bảng ký nghiệm thu (UAT)

| Hạng mục | Người test | Ngày | Pass/Fail | Ghi chú |
|----------|------------|------|-----------|---------|
| Phase 1 Core | | | | |
| Phase 2 Vận hành | | | | |
| Phase 3 Nâng cao | | | | |
| Phase 4 Premium | | | | |
| Test tự động 38 | | | | |
| Build FE | | | | |

---

## 10. Xử lý sự cố thường gặp

| Vấn đề | Cách xử lý |
|--------|------------|
| API không kết nối | Kiểm tra `dotnet run`, cổng 5149, firewall |
| Login lỗi | DB seed; user `admin`; MK `Sample@123` |
| File không mở | Thư mục `QLTBYT.API/FILELUU` hoặc `Captures` |
| Build API khóa file | Tắt process API đang chạy |
| Tiếng Việt DB cũ | `python scripts/refresh_sample_data_vn.py` + restart API |

---

*Tài liệu này là bản tổng hợp; khi nghiệm thu chi tiết từng phase, dùng thêm file hướng dẫn tương ứng ở mục đầu.*