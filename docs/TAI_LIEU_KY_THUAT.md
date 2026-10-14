# Tài liệu kỹ thuật — QLTBYT_NEW

**Stack:** React 18 + TypeScript + Vite + Ant Design · .NET 7 Web API · EF Core · SQL Server · JWT  
**Database:** giữ schema legacy `db_QLTBYT` (bảng `Quanlythietbi`, `Taikhoan`, `LogThietbi`…)

---

## 1. Kiến trúc

```
React SPA  ←── HTTP/REST + JWT Bearer ──→  .NET 7 API  ←──→  SQL Server
```

| Thành phần | Thư mục |
|------------|---------|
| Frontend | `qltbyt-frontend/` |
| Backend | `QLTBYT.API/` |
| Tests | `QLTBYT.API.Tests/` |
| Scripts hỗ trợ | `scripts/` |

**Nguyên tắc:** Controller mỏng → Service → EF Core; không expose entity ra API (DTO); UI tiếng Việt.

---

## 2. Phân quyền

### 2.1. Mapping LoaiTK → JWT role (`RoleMapper`)

| LoaiTK | Tên (legacy) | Role JWT |
|--------|--------------|----------|
| 0 | Ban lãnh đạo | Admin |
| 1 | Tổ kho, thiết bị | Technician |
| 2 | Kỹ sư khu vực | User |
| 3 | Nhân viên mạng lưới | User |
| `Admin=1` | — | Admin (ưu tiên) |

Claim JWT quan trọng: `role`, `LoaiTK`, `MaDV`, `NameIdentifier`.

### 2.2. Policy API (`Program.cs`)

| Policy | Ai được |
|--------|---------|
| `AdminOnly` | Admin |
| `TechnicianAndAbove` | Admin, Technician |
| `AllUsers` | Admin, Technician, User |
| `ReportsAccess` | LoaiTK 0,1,2 (chặn 3) |
| `BiddingAccess` | LoaiTK 0,1 |
| `WarehouseAccess` | LoaiTK 0 hoặc 3 |

### 2.3. Menu frontend (`permissions.ts` + `MainLayout`)

| Chức năng | Điều kiện |
|-----------|-----------|
| Kiểm kê `/asset-inventory` | Mọi user đăng nhập |
| Báo cáo `/reports` | `canAccessReports` (LoaiTK ≤ 2) |
| Gói thầu | LoaiTK ≤ 1 |
| Kho TVT | LoaiTK 0 hoặc 3 |
| Nhân sự / hồ sơ / ký số | Admin hoặc Technician |
| Đơn vị / danh mục / TK / log hệ thống | Admin |

**Phạm vi dữ liệu:** Admin & Technician thấy toàn bộ thiết bị; User lọc theo `MaDV` (`MustFilterByOrganization`).

**Mật khẩu:** BCrypt; login bridge chấp nhận plain text legacy một lần rồi hash lại.

---

## 3. Module & API chính

| Module | Route UI | API gốc |
|--------|----------|---------|
| Auth | `/login`, `/profile`… | `/api/auth/*` |
| Dashboard | `/dashboard` | `/api/dashboard/*` |
| Thiết bị | `/devices` | `/api/devices/*` |
| Sự cố | `/events` | `/api/events/*` |
| Báo cáo | `/reports` | `/api/reports/*`, `/api/reports/baocao/*` |
| Kiểm kê | `/asset-inventory` | `/api/asset-inventory/*` |
| Nhật ký SD | `/usage-logs` | `/api/usage-logs`, `/api/devices/:id/usage-logs` |
| Gói thầu | `/bidding` | `/api/bidding` |
| Hồ sơ PL | `/legal-documents` | `/api/legal-documents` |
| Kho | `/warehouse` | `/api/warehouse/*` |
| Đơn vị | `/organizations` | `/api/organizations` |
| Nhân sự | `/personnel` | `/api/personnel` |
| User | `/users` | `/api/users` |
| Danh mục | `/admin/master` | `/api/master/*`, `/api/admin/master/*` |
| Audit | `/admin/system-logs` | `/api/admin/system-audit-logs` |
| Thông báo | Header | `/api/notifications` |
| Health | — | `/api/health`, `/api/health/database` |

### In phiếu thiết bị (thay DevExpress `.repx`)

| Loại | Legacy | API |
|------|--------|-----|
| PSC | `viewrp?type=0` / `rpPhieusuachua` | `GET /api/reports/devices/{id}/print/repair` |
| BCKT | `type=1` / `prBCKT` | `.../print/handover` |
| PNT | `type=2` / `rpPhieuNT` | `.../print/acceptance` |

Mẫu PDF tham chiếu: `docs/report-samples/`. Header cấu hình `PrintSettings` trong appsettings.

### File sự cố (parity legacy)

| Loại | Thư mục | Endpoint |
|------|---------|----------|
| Ảnh sự cố | `FILELUU/{id}/Hinh_anh_su_co/` | `POST /api/devices/{id}/incident-files` (`kind=image`) |
| Báo cáo SC | `FILELUU/{id}/BAO_CAO_SU_CO/` | `POST /api/events/{id}/files` (`kind=report`) |

Static: `/files/*`, `/hdbaotri/*`, `/captures/*`.

---

## 4. Quy tắc nghiệp vụ quan trọng

### Trạng thái thiết bị (`DMTrangthai`)

- 1 = Đang hoạt động  
- 2 / 8 = Không hoạt động / Hỏng  
- 3 = Chưa sử dụng  

### Sự kiện (`LogThietbi`)

**Loại:** 1 Báo hỏng · 2 Kiểm tra · 3 Yêu cầu khác · 4 Chuyển giao · 5 Đang xử lý  

**Trạng thái xử lý (hệ mới thường dùng):** chờ xử lý / đã xử lý (map trong service).

### Hồ sơ pháp lý (`ThongtinHS`)

- Cột `MaThietbi` legacy thường lưu **ID thiết bị**, không phải `MaTB`.
- UI resolve ID → tên/mã thiết bị khi hiển thị.
- `Quanlythietbi.IDHoso` có thể vừa là ID hồ sơ vừa là tên file HDBaotri — xử lý cẩn thận khi link.

### Nhật ký sử dụng

- Bảng `InputByDate` — upsert theo mã TB / ngày (`UsageLogService`).
- Khác với lịch sử chuyển giao trên chi tiết TB.

### Cảnh báo hết hạn kiểm định

- Job nền `DeviceExpiryNotificationJob` (không gắn login).
- Flag `MaKT`: `0` chưa gửi / `1` đã gửi.

---

## 5. Map chức năng cũ → mới (tóm tắt)

| ASPX cũ | Route mới |
|---------|-----------|
| `login.aspx` | `/login` |
| `Dashboard.aspx` | `/dashboard` |
| `Timkiem.aspx` | `/devices` |
| `Themdulieu` / `edit` / `view` | `/devices/new`, `/:id/edit`, `/:id` |
| `guiyc` / `xuly` / `log` | Modal + `/events` |
| `baocao` / `baocaoTS` | `/reports`, `/asset-inventory` |
| `goithau` | `/bidding` |
| `thongtinhs` / `hoso` | `/legal-documents` |
| `Nhaphanghoa` / `Xuathanghoa` | `/warehouse` |
| `viewrp.aspx` | In phiếu HTML trên chi tiết TB |
| `taikhoan` / `donvi` / `quanlynhansu` | `/users`, `/organizations`, `/personnel` |
| `scanqr` | `/scan-qr` |
| `sign` | `/sign-document` |
| `CaptureNhatky` | Cố ý không mang đủ — dùng `/usage-logs` + camera chi tiết |

Chi tiết từng màn hình: xem lịch sử commit / code `MainLayout`, `App.tsx`.

---

## 6. Cấu trúc code quan trọng

### Backend

```
QLTBYT.API/
  Controllers/     # endpoints
  Services/        # nghiệp vụ
  Models/Domain/   # entity EF
  Models/DTOs/     # request/response
  Helpers/         # RoleMapper, DevicePrintTemplates, JwtHelper…
  Data/AppDbContext.cs
  Program.cs       # DI, JWT, CORS, policies, static files
```

### Frontend

```
qltbyt-frontend/src/
  api/axiosClient.ts   # JWT interceptor, base URL
  pages/               # màn hình
  components/          # modal, layout
  store/authStore.ts   # Zustand + persist
  utils/permissions.ts # quyền menu theo LoaiTK
  utils/apiBase.ts     # origin API production
```

---

## 7. Phát triển local

```powershell
# API
cd QLTBYT.API
dotnet run
# http://localhost:5149  Swagger: /swagger

# FE
cd qltbyt-frontend
npm install
npm run dev
# http://localhost:5173 — proxy /api,/files,/captures
```

- Connection: `appsettings.Development.json` (không commit).
- Development có thể seed `Sample@123` (`admin` / `kythuat` / `user`) và `TB-DEMO-*`.
- Test: `dotnet test QLTBYT.API.Tests\QLTBYT.API.Tests.csproj`

### SMTP / Firebase

- `SmtpSettings.Enabled`, `FirebaseSettings.Enabled` trong appsettings.
- Mặc định Development: tắt. Bật khi có credential thật (`NOTIFICATION` / IncidentAlertService).

### Index SQL gợi ý

Xem `docs/sql/phase4_indexes.sql` (hạn kiểm định, kho…). Chạy thủ công trên DB khi cần tối ưu.

---

## 8. Lưu ý kỹ thuật

1. Một số SQL Server cũ không hỗ trợ `OFFSET` tốt — paging có thể materialize rồi `Skip/Take` in-memory (ví dụ audit log).
2. `VITE_API_BASE_URL` khi FE và API khác origin (production tách cổng).
3. Không commit secrets: `appsettings.*.json` thật, `.env*`, `firebase-service-account.json`.
4. Sau đổi `RoleMapper`: user phải login lại.
