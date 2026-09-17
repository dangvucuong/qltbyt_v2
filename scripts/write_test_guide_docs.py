# -*- coding: utf-8 -*-
"""Tạo docs/HUONG_DAN_TEST_PHASE1_PHASE2.md và cập nhật checklist UAT."""
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
DOCS = ROOT / "docs"

MAIN = r"""# Hướng dẫn test chi tiết Phase 1 & Phase 2 (trước khi sang Phase 3)

**Phiên bản:** 1.0 · Cập nhật theo codebase QLTBYT_NEW  
**Đối tượng:** QA, BA, kỹ thuật viên triển khai, dev nội bộ  
**Mục tiêu:** Nghiệm thu đầy đủ Core (Phase 1) và Vận hành (Phase 2) trước khi bắt đầu Phase 3 (báo cáo Excel, gói thầu, QR, phân quyền UI nâng cao…).

---

## 1. Tiêu chí “sẵn sàng Phase 3”

| Hạng mục | Yêu cầu |
|----------|---------|
| Tự động | `dotnet test` — **29** test passed (28 Phase 1 + 1 Phase 2 events) |
| Phase 1 UI | Đăng nhập, dashboard, CRUD thiết bị, upload file, phân quyền đơn vị — **Pass** trên 3 role |
| Phase 2 | Báo sự cố → danh sách → xử lý; thông báo; API users/personnel/usage-logs — **Pass** theo checklist |
| Ghi nhận | Bảng kết quả cuối tài liệu đã điền, blocker Phase 3 được liệt kê |

---

## 2. Chuẩn bị môi trường

### 2.1 Yêu cầu phần mềm

- Windows + **SQL Server** (instance dev, ví dụ `CUONG-PC\SQLSERVER`)
- **.NET 7 SDK**
- **Node.js 18+** và npm
- (Tuỳ chọn) Visual Studio 2022 để build API

### 2.2 Database

1. Tạo database **`db_QLTBYT_NEW`** (hoặc tên trong `appsettings.Development.json`).
2. Restore/script từ `dbqltbyt.sql` nếu môi trường mới.
3. Kiểm tra connection string trong `QLTBYT.API/appsettings.Development.json` (key `DefaultConnection`).

### 2.3 Chạy API (Development)

```powershell
cd D:\DVC\PROJECT\QLTBYT_NEW\QLTBYT.API
dotnet run
```

- URL mặc định: **http://localhost:5149**
- Swagger: **http://localhost:5149/swagger**
- Môi trường **Development** tự seed dữ liệu mẫu (`DevelopmentDataSeeder` + `SampleDataVietnameseRefresher`).

**Lưu ý:** Nếu `dotnet build` báo file `.exe` bị khóa, tắt process API đang chạy trước khi build lại.

### 2.4 Chạy Frontend

```powershell
cd D:\DVC\PROJECT\QLTBYT_NEW\qltbyt-frontend
npm install
npm run dev
```

- URL: **http://localhost:5173**
- Proxy Vite: `/api` và `/files` → `http://localhost:5149`

### 2.5 Tài khoản demo

Mật khẩu chung: **`Sample@123`**

| Username | Role (UI) | LoaiTK | Phạm vi đơn vị (MaDV) | Ghi chú |
|----------|-----------|--------|------------------------|---------|
| `admin` | Admin | 1 | Toàn hệ thống | CRUD đầy đủ, xóa thiết bị |
| `kythuat` | Technician | 2 | Toàn hệ thống | Thêm/sửa TB, upload, xử lý sự cố |
| `user` | User | 3 | **Đơn vị đầu tiên** trong DB (thường Khoa Nội) | Chỉ thấy TB của đơn vị được gán |

Chi tiết thiết bị demo: `docs/SAMPLE_DATA.md`.

| Mã TB | Đơn vị (thường) | User `user` thấy? |
|-------|-----------------|-------------------|
| TB-DEMO-001 | Đơn vị 1 | Có |
| TB-DEMO-002 | Đơn vị 2 | **Không** (dùng admin/kythuat để test) |
| TB-DEMO-003 | Đơn vị 1 (TB con của 001) | Có |

### 2.6 Làm sạch / làm lại dữ liệu mẫu tiếng Việt

```powershell
python D:\DVC\PROJECT\QLTBYT_NEW\scripts\refresh_sample_data_vn.py
```

Sau đó restart API (Development) để seeder chạy lại.

---

## 3. Kiểm thử tự động (bắt buộc)

```powershell
cd D:\DVC\PROJECT\QLTBYT_NEW
dotnet test QLTBYT.API.Tests\QLTBYT.API.Tests.csproj
```

**Kỳ vọng:** `Passed: 29`, `Failed: 0`.

| Nhóm test | Nội dung chính |
|-----------|----------------|
| Step0 | Health, schema DB |
| Phase1 | Auth, devices CRUD, scope đơn vị, validators, mapping |
| Phase2 | User tạo sự cố TB-DEMO-001 → kỹ thuật resolve |

Chỉ chạy Phase 2:

```powershell
dotnet test QLTBYT.API.Tests\QLTBYT.API.Tests.csproj --filter "Phase2"
```

---

## 4. Phase 1 — Test chi tiết (Core)

Ánh xạ bước triển khai 1.1–1.18 (`tlchuyendoi.md`).

### 4.1 Auth & bảo mật (1.5, 1.15)

| ID | Bước test | Cách thực hiện | Kỳ vọng |
|----|-----------|----------------|---------|
| P1-A01 | Login sai MK | UI: `/login`, username `admin`, MK sai | Thông báo lỗi, không vào app |
| P1-A02 | Login admin | `admin` / `Sample@123` | Chuyển `/dashboard`, header hiện tên |
| P1-A03 | Route guard | Chưa login, mở `/devices` | Redirect `/login` |
| P1-A04 | Token 401 | Xóa token (đăng xuất) hoặc hết hạn | Tự về login |
| P1-A05 | Đăng xuất | Nút **Đăng xuất** | Về login, không gọi API được |
| P1-A06 | API login | Swagger `POST /api/auth/login` | `success: true`, có `token` |
| P1-A07 | Me | `GET /api/auth/me` (Bearer) | Thông tin user khớp role |
| P1-A08 | Đổi MK | `POST /api/auth/change-password` | MK cũ sai → 400; đúng → success (đổi lại MK demo sau test) |

### 4.2 Dashboard (1.17)

| ID | Bước test | Kỳ vọng |
|----|-----------|---------|
| P1-D01 | Mở `/dashboard` | Không lỗi, có số **Tổng thiết bị** ≥ 3 (có TB-DEMO) |
| P1-D02 | Biểu đồ | Có dữ liệu trạng thái / loại (Recharts) |
| P1-D03 | API | `GET /api/dashboard/summary?expiringWithinDays=90` | `totalDevices`, `byStatus`, `byType`, `expiringInspectionCount` |

### 4.3 Danh mục & đơn vị (1.7, 1.8)

| ID | Endpoint | Kỳ vọng |
|----|----------|---------|
| P1-M01 | `GET /api/master/device-types` | Danh sách loại TB, tiếng Việt có dấu |
| P1-M02 | `GET /api/master/status` | Trạng thái: Đang hoạt động; Không hoạt động… |
| P1-M03 | `GET /api/master/countries` | Có Việt Nam |
| P1-M04 | `GET /api/organizations` | Danh sách khoa/phòng |

UI: form **Thêm/Sửa thiết bị** — dropdown loại, trạng thái, đơn vị load đúng.

### 4.4 Danh sách & tìm kiếm thiết bị (1.9)

| ID | Bước test | Kỳ vọng |
|----|-----------|---------|
| P1-L01 | `/devices` | Bảng có cột mã, tên, loại, đơn vị, trạng thái |
| P1-L02 | Tìm `TB-DEMO` | ≥ 3 dòng |
| P1-L03 | Lọc đơn vị / trạng thái / loại | Kết quả thay đổi hợp lý |
| P1-L04 | Phân trang | `page`, `pageSize` — API `GET /api/devices?page=1&pageSize=5` |
| P1-L05 | Sắp hết hạn KĐ | TB-DEMO-002 thường trong nhóm sắp hết hạn (≈ 20 ngày) |

### 4.5 CRUD thiết bị (1.10–1.12)

**Admin hoặc kythuat:**

| ID | Bước test | Kỳ vọng |
|----|-----------|---------|
| P1-C01 | Thêm TB | `/devices/new` — tạo mã unique, bắt buộc loại/đơn vị/trạng thái → lưu OK |
| P1-C02 | Chi tiết | Click mã → `/devices/{id}` — mô tả đủ trường |
| P1-C03 | Sửa | `/devices/{id}/edit` — cập nhật tên/ghi chú → lưu OK |
| P1-C04 | Thiết bị con | TB-DEMO-003 — tab/con list; API `GET /api/devices/{id}/children` |
| P1-C05 | Xóa (admin) | `DELETE /api/devices/{id}` — chỉ admin; TB không demo nếu có thể |

**User (`user`):**

| ID | Bước test | Kỳ vọng |
|----|-----------|---------|
| P1-C06 | Danh sách scope | Thấy TB-DEMO-001/003, **không** thấy 002 (nếu 002 ở đơn vị khác) |
| P1-C07 | Sửa subset | Sửa TB trong scope — OK theo quyền UI |
| P1-C08 | Xóa TB | 403 hoặc không có nút xóa |

### 4.6 Upload file (1.13)

| ID | Bước test | Kỳ vọng |
|----|-----------|---------|
| P1-F01 | Upload | Chi tiết TB — **Tải lên tài liệu** (kythuat/admin) |
| P1-F02 | Hiển thị | File trong list, link `/files/...` mở được |
| P1-F03 | User | User có thể bị chặn upload (policy TechnicianAndAbove) |

### 4.7 Phân quyền (1.6)

| ID | Kiểm tra | Kỳ vọng |
|----|----------|---------|
| P1-R01 | User + TB ngoài scope | `GET /api/devices/{id}` TB-DEMO-002 → 404 hoặc không trong list |
| P1-R02 | Technician | Không `DELETE` device (403) |
| P1-R03 | Admin | `GET /api/users` — **403** (chưa Phase 2 UI; API Phase 2) |

### 4.8 Phase 1 — Smoke Swagger (không UI)

1. Login → copy JWT.
2. Authorize Swagger: `Bearer {token}`.
3. Gọi lần lượt: `devices`, `devices/{id}`, `master/device-types`, `dashboard/summary`.

---

## 5. Phase 2 — Test chi tiết (Vận hành)

### 5.1 Sự cố / yêu cầu (2.1–2.3)

| ID | Bước test | Role | Kỳ vọng |
|----|-----------|------|---------|
| P2-E01 | Báo sự cố UI | `user` | TB-DEMO-001 → **Báo sự cố** → gửi (loại, nội dung) → thông báo thành công |
| P2-E02 | API tạo | `user` | `POST /api/devices/{id}/events` body: `eventTypeId`, `content`, `keyword` |
| P2-E03 | Danh sách | `kythuat` | `/events` — bản ghi **Chờ xử lý** |
| P2-E04 | API list | Bearer | `GET /api/events?statusId=1&page=1&pageSize=50` |
| P2-E05 | Xử lý UI | `kythuat` | **Xử lý** → nhập nội dung → trạng thái **Đã xử lý** |
| P2-E06 | API resolve | `kythuat` | `PUT /api/events/{id}` — `resolutionContent`, `cost` |
| P2-E07 | User scope | `user` | Sự cố TB ngoài scope không resolve được |

**Loại sự cố (legacy Loai):** 1 Báo hỏng, 2 Kiểm tra, 3 Yêu cầu khác, 4 Chuyển giao.  
**Trạng thái:** 1 Chờ xử lý, 2 Đã xử lý.

### 5.2 Event keywords (2.4)

| ID | Bước test | Kỳ vọng |
|----|-----------|---------|
| P2-K01 | `GET /api/events/keywords` | Danh sách (kythuat+) |
| P2-K02 | `POST /api/events/keywords` | Thêm keyword mới |
| P2-K03 | Master | `GET /api/master/event-keywords` | Đồng bộ danh mục form cũ |

### 5.3 Nhật ký sử dụng (2.5)

| ID | Bước test | Kỳ vọng |
|----|-----------|---------|
| P2-U01 | Ghi log | `POST /api/usage-logs` — `deviceCode`: `TB-DEMO-001`, `inputDate`, `hours`/`sessions` |
| P2-U02 | Đọc log | `GET /api/usage-logs?deviceCode=TB-DEMO-001` |
| P2-U03 | Scope | `user` — TB ngoài scope → 400/403 |

*(UI nhật ký chưa có — test qua Swagger.)*

### 5.4 Thông báo (2.10)

| ID | Bước test | Kỳ vọng |
|----|-----------|---------|
| P2-N01 | Sau P2-E01 | `kythuat`/`admin` — chuông header có thông báo mới |
| P2-N02 | API | `GET /api/notifications` |
| P2-N03 | Đọc | Click item hoặc `PUT /api/notifications/{id}/read` — `isRead: true` |

### 5.5 Tài khoản (2.11)

| ID | Bước test | Role | Kỳ vọng |
|----|-----------|------|---------|
| P2-T01 | List users | `admin` | `GET /api/users` — có admin, kythuat, user |
| P2-T02 | CRUD | `admin` | POST/PUT/DELETE user test (không xóa admin đang login) |
| P2-T03 | Non-admin | `kythuat` | `GET /api/users` → 403 |
| P2-T04 | Profile | Mọi user | `GET /api/auth/me`, đổi MK (cẩn thận MK demo) |

*(UI quản lý tài khoản chưa có — test API/Swagger.)*

### 5.6 Nhân sự (2.12)

| ID | Bước test | Kỳ vọng |
|----|-----------|---------|
| P2-P01 | `GET /api/personnel` | kythuat+ |
| P2-P02 | Thêm/sửa/xóa | POST/PUT/DELETE — audit ghi log |

### 5.7 Audit (2.13)

| ID | Kiểm tra | Kỳ vọng |
|----|----------|---------|
| P2-A01 | Sau tạo sự cố / user / personnel | Bảng `Loghethong` (entity SystemAuditLog) có bản ghi |

### 5.8 Email / Push / Job (2.7–2.9)

| ID | Nội dung | Kỳ vọng Development |
|----|----------|----------------------|
| P2-I01 | Firebase push | **Stub** — không gửi thật (`DevelopmentIncidentAlertService`) |
| P2-I02 | MailKit SMTP | **Chưa cấu hình** — không gửi email thật |
| P2-I03 | Job 6:00 | `DeviceExpiryNotificationJob` chạy nền — có TB sắp hết hạn → thông báo nội bộ (kiểm log API hoặc chuông) |

### 5.9 Chưa làm / Phase 3 (2.6)

- Nhật ký text/ảnh legacy (`CaptureNhatky`) — **deprecated**, không nghiệm thu Phase 2.

---

## 6. Kịch bản E2E gợi ý (30 phút)

1. **Admin:** login → dashboard → tìm TB-DEMO → thêm TB test → upload file → xóa TB test (nếu cần).
2. **User:** login → chỉ thấy TB đơn vị mình → TB-DEMO-001 → **Báo sự cố**.
3. **Kỹ thuật:** chuông thông báo → `/events` → **Xử lý** → kiểm tra đã xử lý.
4. **Admin:** Swagger — users list, usage-log, personnel.
5. `dotnet test` — 29 passed.

---

## 7. Xử lý sự cố thường gặp

| Vấn đề | Cách xử lý |
|--------|------------|
| API không kết nối DB | Kiểm tra SQL Server, connection string, firewall |
| Frontend 404 API | API phải chạy 5149; kiểm tra proxy Vite |
| Không có TB-DEMO | `ASPNETCORE_ENVIRONMENT=Development`, restart API |
| Trạng thái không dấu | `python scripts/refresh_sample_data_vn.py` + restart API |
| Build MSB3021 file locked | Tắt `QLTBYT.API` đang chạy |
| CS1056 `\0` trong .cs | File UTF-16 — convert UTF-8 hoặc dùng `scripts/gen_phase2_rest.py` |

---

## 8. Bảng ghi kết quả nghiệm thu

### Phase 1

| ID | Pass/Fail | Người test | Ngày | Ghi chú |
|----|-----------|------------|------|---------|
| P1-A01–A08 | | | | |
| P1-D01–D03 | | | | |
| P1-M01–M04 | | | | |
| P1-L01–L05 | | | | |
| P1-C01–C08 | | | | |
| P1-F01–F03 | | | | |
| P1-R01–R03 | | | | |
| `dotnet test` (28 Phase1) | | | | |

### Phase 2

| ID | Pass/Fail | Người test | Ngày | Ghi chú |
|----|-----------|------------|------|---------|
| P2-E01–E07 | | | | |
| P2-K01–K03 | | | | |
| P2-U01–U03 | | | | |
| P2-N01–N03 | | | | |
| P2-T01–T04 | | | | |
| P2-P01–P02 | | | | |
| P2-A01 | | | | |
| P2-I01–I03 | | | | |
| `dotnet test` Phase2 | | | | |

### Quyết định

- [ ] **Đạt** — cho phép bắt đầu Phase 3  
- [ ] **Chưa đạt** — blocker: _________________________________

**Người phê duyệt:** _________________ **Ngày:** ___________

---

## 9. Tài liệu liên quan

| File | Nội dung |
|------|----------|
| `docs/SAMPLE_DATA.md` | Tài khoản & TB demo |
| `docs/PHASE1_UAT.md` | Checklist rút gọn Phase 1 |
| `docs/PHASE2_UAT.md` | Checklist rút gọn Phase 2 |
| `docs/PHASE2_STATUS.md` | Trạng thái triển khai Phase 2 |
| `tlchuyendoi.md` | Map bước 1.x / 2.x / 3.x |
| `QLTBYT_Technical_Spec.md` | Đặc tả kỹ thuật |

"""

PHASE1_SHORT = """# Phase 1 — Checklist UAT (rút gọn)

Hướng dẫn **chi tiết**: [HUONG_DAN_TEST_PHASE1_PHASE2.md](./HUONG_DAN_TEST_PHASE1_PHASE2.md)

## Môi trường

- [ ] API `http://localhost:5149`, Swagger OK
- [ ] Frontend `http://localhost:5173`
- [ ] `dotnet test` — 28+ tests passed

## Checklist nhanh

- [ ] Login / logout / 401 → login (`admin` / `Sample@123`)
- [ ] Dashboard: tổng TB, biểu đồ, sắp hết hạn
- [ ] Thiết bị: tìm TB-DEMO, lọc, thêm/sửa/chi tiết/con, upload (kythuat+)
- [ ] User `user`: scope đơn vị, không xóa TB

Dữ liệu mẫu: [SAMPLE_DATA.md](./SAMPLE_DATA.md)
"""

PHASE2_SHORT = """# Phase 2 — Checklist UAT (rút gọn)

Hướng dẫn **chi tiết**: [HUONG_DAN_TEST_PHASE1_PHASE2.md](./HUONG_DAN_TEST_PHASE1_PHASE2.md)

## Môi trường

- [ ] Phase 1 đã pass
- [ ] Tài khoản: `admin`, `kythuat`, `user` — `Sample@123`

## Checklist nhanh

- [ ] User: TB-DEMO-001 → **Báo sự cố**
- [ ] Kỹ thuật: `/events` → xử lý sự cố
- [ ] Chuông thông báo (admin/kythuat)
- [ ] Swagger: `GET /api/users` (admin), `usage-logs`, `personnel`, `events/keywords`
- [ ] `dotnet test --filter Phase2`

Chi tiết API/UI: tài liệu hướng dẫn chính ở trên.
"""

README = """# QLTBYT_NEW — Quản lý thiết bị y tế

- **Backend:** `QLTBYT.API` (.NET 7) — http://localhost:5149
- **Frontend:** `qltbyt-frontend` (React 18 + Vite + Ant Design) — http://localhost:5173
- **Tests:** `QLTBYT.API.Tests` (`dotnet test`)
- **Database:** `db_QLTBYT_NEW` (script `dbqltbyt.sql`)

## Chạy nhanh (Development)

1. Cấu hình DB trong `QLTBYT.API/appsettings.Development.json`
2. API: `cd QLTBYT.API` → `dotnet run`
3. UI: `cd qltbyt-frontend` → `npm install` → `npm run dev`
4. Đăng nhập demo: `admin` / `kythuat` / `user` — mật khẩu `Sample@123`

## Test & nghiệm thu (trước Phase 3)

**Tài liệu chính:** [docs/HUONG_DAN_TEST_PHASE1_PHASE2.md](docs/HUONG_DAN_TEST_PHASE1_PHASE2.md)

- Checklist rút gọn: [docs/PHASE1_UAT.md](docs/PHASE1_UAT.md), [docs/PHASE2_UAT.md](docs/PHASE2_UAT.md)
- Dữ liệu mẫu: [docs/SAMPLE_DATA.md](docs/SAMPLE_DATA.md)
"""

def main():
    DOCS.mkdir(exist_ok=True)
    (DOCS / "HUONG_DAN_TEST_PHASE1_PHASE2.md").write_text(MAIN, encoding="utf-8", newline="\n")
    (DOCS / "PHASE1_UAT.md").write_text(PHASE1_SHORT, encoding="utf-8", newline="\n")
    (DOCS / "PHASE2_UAT.md").write_text(PHASE2_SHORT, encoding="utf-8", newline="\n")
    (ROOT / "README.md").write_text(README, encoding="utf-8", newline="\n")
    print("Written HUONG_DAN_TEST_PHASE1_PHASE2.md, UAT checklists, README")


if __name__ == "__main__":
    main()
