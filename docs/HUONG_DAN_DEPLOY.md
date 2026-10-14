# Hướng dẫn triển khai hệ thống — QLTBYT_NEW

**Đối tượng:** IT / DevOps  
**Môi trường đích điển hình:** Windows Server + IIS + SQL Server

---

## 1. Yêu cầu môi trường

| Thành phần | Yêu cầu |
|------------|---------|
| OS | Windows Server / Windows 10+ |
| Runtime API | [.NET 7 ASP.NET Core Hosting Bundle](https://dotnet.microsoft.com/download/dotnet/7.0) |
| SQL Server | Instance có database `db_QLTBYT` (schema legacy) |
| IIS | Site API + Site Frontend (hoặc reverse proxy) |
| Node.js | ≥ 18 (chỉ máy build FE) |
| Firewall | Mở cổng API (vd. 8016), FE (vd. 8017) |

---

## 2. Chuẩn bị database

1. Backup DB production/legacy trước khi thao tác.
2. Dùng DB hiện có `db_QLTBYT` **hoặc** restore sang instance mới (giữ schema).
3. Connection string mẫu (Production):

```
Server=YOUR_SQL_SERVER;Database=db_QLTBYT;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True
```

4. (Tuỳ chọn) Chạy index gợi ý: `docs/sql/phase4_indexes.sql`.
5. Mật khẩu user: hệ mới dùng BCrypt; lần login đầu với plain text legacy sẽ được nâng cấp hash tự động.

---

## 3. Cấu hình Production (API)

1. Copy `QLTBYT.API/appsettings.Production.json.example` → `appsettings.Production.json` trên server (file này **không** commit).
2. Điền tối thiểu:

| Section | Ý nghĩa |
|---------|---------|
| `ConnectionStrings:DefaultConnection` | SQL Server |
| `JwtSettings:SecretKey` | Chuỗi bí mật ≥ 32 ký tự, **đổi** so với example |
| `Cors:AllowedOrigins` | Origin frontend, vd. `http://IP:8017` |
| `PrintSettings` | Tên bệnh viện / phòng VT trên phiếu in |
| `SmtpSettings` / `FirebaseSettings` | Bật khi cần email/push thật |

3. Biến môi trường IIS (khuyến nghị):

```
ASPNETCORE_ENVIRONMENT=Production
```

Có thể override connection/JWT bằng biến môi trường ASP.NET Core thay vì ghi file.

---

## 4. Publish API

Trên máy build:

```powershell
cd D:\DVC\PROJECT\QLTBYT_NEW
dotnet publish QLTBYT.API\QLTBYT.API.csproj -c Release -o D:\deploy\qltbyt-api
```

Copy sang server (vd. `C:\QLTBYT\api`), đảm bảo có:

- `appsettings.Production.json`
- `web.config` (AspNetCoreModuleV2)
- Thư mục ghi: `logs`, `FILELUU`, `Captures`, `HDBaotri`

### Tạo site IIS (script)

Chạy **PowerShell Admin** trên server:

```powershell
cd C:\path\to\repo\scripts
.\setup-iis-api.ps1 -SitePath "C:\QLTBYT\api" -Port 8016
```

Script tạo App Pool (**No Managed Code**, AlwaysRunning), site HTTP cổng 8016, quyền ghi, rule firewall.

### Kiểm tra API

```
GET http://127.0.0.1:8016/api/health
GET http://127.0.0.1:8016/api/health/database
```

- 502.5 → chưa cài Hosting Bundle hoặc lỗi startup → xem `logs\stdout*.log`, Event Viewer.
- 401 trên API nghiệp vụ là bình thường khi chưa gửi JWT.

---

## 5. Publish Frontend

1. Tạo file môi trường production (máy build), **không** commit secret:

```env
VITE_API_BASE_URL=http://YOUR_SERVER_IP:8016
VITE_APP_TITLE=Quản Lý Thiết Bị Y Tế
```

2. Build:

```powershell
cd qltbyt-frontend
npm ci
npm run build
```

3. Copy nội dung `dist/` lên IIS site FE (vd. cổng **8017**), hoặc nginx static.

4. CORS trên API phải chứa đúng origin FE (`http://IP:8017`).

5. Kiểm tra: mở FE → Đăng nhập → F12 Network thấy gọi đúng host API.

---

## 6. Cấu trúc thư mục file trên server

| Path (dưới thư mục API) | Mục đích |
|-------------------------|----------|
| `FILELUU/` | Tài liệu TB, ảnh/báo cáo sự cố |
| `Captures/` | Ảnh chụp thiết bị |
| `HDBaotri/` | Hợp đồng bảo trì |
| `logs/` | stdout / Serilog |

Cấp quyền **Modify** cho `IIS AppPool\QLTBYT-API-Pool` (script setup đã grant).

Static URL:

- `/files/...` → `FILELUU`
- `/captures/...` → `Captures`
- `/hdbaotri/...` → `HDBaotri`

---

## 7. Checklist go-live

| # | Việc | OK |
|---|------|----|
| 1 | Hosting Bundle .NET 7 đã cài | ☐ |
| 2 | SQL kết nối được từ server API | ☐ |
| 3 | `/api/health` và `/api/health/database` 200 | ☐ |
| 4 | `appsettings.Production.json` đúng CORS + JWT mới | ☐ |
| 5 | FE build với `VITE_API_BASE_URL` đúng | ☐ |
| 6 | Đăng nhập admin / kỹ thuật / user | ☐ |
| 7 | Upload file + mở lại qua `/files` | ☐ |
| 8 | In phiếu từ chi tiết TB | ☐ |
| 9 | Menu đúng LoaiTK (báo cáo / kiểm kê / kho) | ☐ |
| 10 | Backup SQL + thư mục file đã lên lịch | ☐ |

---

## 8. Backup & vận hành

- **SQL:** backup hàng ngày `db_QLTBYT` (full + log theo chính sách bệnh viện).
- **File:** backup `FILELUU`, `Captures`, `HDBaotri`.
- **Cập nhật phiên bản:** publish API/FE mới → recycle App Pool → hard refresh trình duyệt (Ctrl+F5).
- **Sau đổi RoleMapper / JWT secret:** toàn bộ user phải đăng nhập lại.

---

## 9. Sự cố thường gặp

| Hiện tượng | Nguyên nhân / xử lý |
|------------|---------------------|
| FE login 404 | Sai `VITE_API_BASE_URL` hoặc FE chưa rebuild |
| CORS lỗi | Thiếu origin FE trong `Cors:AllowedOrigins` |
| 502.5 | Hosting Bundle / `web.config` / stdout log |
| Menu thiếu sau đổi quyền | Chưa login lại |
| Upload fail | Thiếu quyền ghi thư mục FILELUU |
| DB timeout | Firewall SQL / connection string / SQL Browser |

---

## 10. Tài liệu liên quan

- Hướng dẫn dùng: [`HUONG_DAN_SU_DUNG.md`](./HUONG_DAN_SU_DUNG.md)
- Kỹ thuật / API / phân quyền: [`TAI_LIEU_KY_THUAT.md`](./TAI_LIEU_KY_THUAT.md)
- Mẫu PDF in phiếu: [`report-samples/`](./report-samples/)
