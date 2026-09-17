# 📋 TÀI LIỆU KỸ THUẬT NÂNG CẤP HỆ THỐNG QLTBYT
## Quản Lý Thiết Bị Y Tế — Phiên Bản Mới (React + .NET 7 REST API)

> **Mục đích tài liệu:** Cung cấp toàn bộ thông tin kỹ thuật để Cursor AI thực hiện nâng cấp project từ ASP.NET WebForms (VB.NET) + DevExpress sang React (Frontend) + .NET 7 REST API (Backend).

---

## 1. TỔNG QUAN HỆ THỐNG HIỆN TẠI

### 1.1 Công nghệ cũ
| Thành phần | Công nghệ |
|---|---|
| Frontend | ASP.NET WebForms (.aspx) + VB.NET |
| UI Components | DevExpress v23.1 (ASPxGrid, ASPxChart, ASPxUpload...) |
| Backend | ASP.NET Web Service (.asmx) - `service.vb` |
| Database | SQL Server (db_QLTBYT) |
| PDF Signing | iTextSharp |
| Push Notification | Firebase FCM (Legacy API) |
| Email | SMTP Gmail |
| QR Code | QRCoder library |

### 1.2 Vấn đề với hệ thống cũ
- License DevExpress đắt tiền và phụ thuộc vendor
- Code VB.NET lẫn lộn UI và business logic (code-behind)
- Không có API chuẩn, không thể tích hợp mobile app dễ dàng
- SQL Injection risk trong nhiều method của `service.vb` (string concatenation)
- Mật khẩu lưu plain text trong database (bảng `Taikhoan`)
- Không có JWT authentication
- Session-based, không stateless
- Firebase FCM dùng Legacy API (deprecated)

---

## 2. KIẾN TRÚC MỚI ĐỀ XUẤT

```
┌─────────────────────────────────────────────────────┐
│                    FRONTEND                          │
│  React 18 + TypeScript + Vite                       │
│  Ant Design hoặc MUI (thay DevExpress)              │
│  React Query (data fetching)                        │
│  Zustand (state management)                         │
│  React Router v6                                    │
│  Recharts / Chart.js (thay DevExpress Charts)       │
│  React-PDF / PDF.js                                 │
│  React QR Code                                      │
└──────────────────┬──────────────────────────────────┘
                   │ HTTP/REST (JSON)
                   │ JWT Bearer Token
┌──────────────────▼──────────────────────────────────┐
│                    BACKEND                           │
│  .NET 7 Web API (C#)                                │
│  ASP.NET Core Controllers                           │
│  Entity Framework Core 7                            │
│  AutoMapper                                         │
│  FluentValidation                                   │
│  JWT Authentication (Microsoft.AspNetCore.Auth.JWT) │
│  MailKit (thay SmtpClient cũ)                       │
│  Firebase Admin SDK (Push Notification)             │
│  QRCoder.NET                                        │
│  Serilog (Logging)                                  │
│  Swagger / OpenAPI                                  │
└──────────────────┬──────────────────────────────────┘
                   │ EF Core / Dapper
┌──────────────────▼──────────────────────────────────┐
│               SQL SERVER DATABASE                    │
│  db_QLTBYT (giữ nguyên schema, refactor dần)       │
└─────────────────────────────────────────────────────┘
```

---

## 3. DATABASE SCHEMA (Phân tích từ dbqltbyt.sql)

### 3.1 Danh sách bảng chính

#### Bảng `Quanlythietbi` — **CORE TABLE** (Thiết bị y tế)
```sql
ID              INT IDENTITY PK
MaTB            NVARCHAR(50)       -- Mã thiết bị
MaKT            NVARCHAR(50)       -- Mã kiểm tra (0=chưa gửi email, 1=đã gửi)
Madinhdanh      NVARCHAR(100)      -- Mã định danh
TenTB           NVARCHAR(MAX)      -- Tên thiết bị
Loai            INT                -- FK -> DMLoaiTB.MaloaiTB
Kyhieu          NVARCHAR(?)        -- Ký hiệu
Serial          NVARCHAR(?)        -- Số serial
ThongsoKT       NVARCHAR(?)        -- Thông số kỹ thuật
Ngaytiepnhan    DATE               -- Ngày tiếp nhận
Donvibangiao    NVARCHAR(?)        -- Đơn vị bàn giao (FK -> QuanlyDV)
Donvisudung     NVARCHAR(?)        -- Đơn vị sử dụng (FK -> QuanlyDV)
Donvicungcap    NVARCHAR(?)        -- Đơn vị cung cấp/sửa chữa (FK -> QuanlyDV)
Donvibaotri     NVARCHAR(?)        -- Đơn vị bảo trì
Nguonmuasam     NVARCHAR(?)        -- Nguồn mua sắm/vốn
Nguoiquanly     NVARCHAR(?)        -- Người quản lý (FK -> QuanlyNhansu)
Thoigianbh      NVARCHAR(?)        -- Thời gian bảo hành (tháng)
HangSX          NVARCHAR(?)        -- Hãng sản xuất
NuocSX          INT/NVARCHAR(?)    -- Nước sản xuất (FK -> Country)
NamSX           NVARCHAR(?)        -- Năm sản xuất
Vattudikem      NVARCHAR(?)        -- Vật tư đi kèm
MocTGBH         DATETIME           -- Mốc thời gian bảo hành hết
MocTGBT         DATETIME           -- Mốc thời gian bảo trì
MocTGHieuchuan  DATETIME           -- Mốc thời gian hiệu chuẩn
MocTGKiemdinh  DATETIME           -- Mốc thời gian kiểm định (hankiemdinh)
hankiemdinh    DATE               -- Hạn kiểm định
Trangthai       INT                -- FK -> DMTrangthai.MaTrangthai
IDHoso          NVARCHAR(?)        -- ID hồ sơ liên quan
idGoithau       NVARCHAR(?)        -- FK -> Goithau.ID
Soluong         INT                -- Số lượng
Dongia          FLOAT              -- Đơn giá
Thanhtien       FLOAT              -- Thành tiền/Nguyên giá
Ghichu          NVARCHAR(MAX)      -- Ghi chú
NDXuly          NVARCHAR(?)        -- Nội dung xử lý
ChiphiXuly      FLOAT              -- Chi phí xử lý
Phanloai        NVARCHAR(?)        -- Phân loại/Chủng loại
Thietbichinh    INT                -- FK -> Quanlythietbi.ID (thiết bị cha)
Vitri           NVARCHAR(?)        -- Vị trí đặt thiết bị
Phannhom        NVARCHAR(?)        -- Phân nhóm
Ngaycapnhat     DATETIME           -- Ngày cập nhật cuối
```

#### Bảng `Taikhoan` — Tài khoản người dùng
```sql
ID              INT IDENTITY PK
Tendangnhap     NVARCHAR(50)       -- Username
Matkhau         NVARCHAR(50)       -- ⚠️ Plain text (cần hash trong version mới)
Admin           INT                -- Quyền admin (1=admin, 0=user)
Hoten           NVARCHAR(200)      -- Họ tên
Dienthoai       NVARCHAR(50)
Email           NVARCHAR(50)
RegID           NVARCHAR(500)      -- Firebase Push notification token
LoaiTK          INT                -- Loại tài khoản (1=admin, 2=kỹ thuật, 3=user)
MaDV            NVARCHAR(500)      -- Mã đơn vị (có thể nhiều DV, ngăn cách ";")
```

#### Bảng `QuanlyDV` — Đơn vị
```sql
ID                      INT IDENTITY PK
Tendonvi                NVARCHAR(500)
Masothue                NVARCHAR(50)
Diachi                  NVARCHAR(MAX)
Dienthoai               NVARCHAR(50)
Email                   NVARCHAR(50)
Nguoichiutrachnhiem     NVARCHAR(50)
MaLoai                  INT            -- FK -> DanhmucDV.MaloaiDV
```

#### Bảng `QuanlyNhansu` — Nhân sự
```sql
ID          INT IDENTITY PK
Hoten       NVARCHAR(50)
SoCMND      NVARCHAR(50)
Dienthoai   NVARCHAR(50)
Email       NVARCHAR(50)
MaDV        INT            -- FK -> QuanlyDV
LoaiNhansu  INT
IDTaikhoan  INT            -- FK -> Taikhoan
```

#### Bảng `LogThietbi` — Lịch sử sự kiện thiết bị
```sql
ID          INT IDENTITY PK
MaTB        INT            -- FK -> Quanlythietbi.ID
Loai        INT            -- 1=báo hỏng, 2=đề nghị kiểm tra, 3=yêu cầu khác, 4=chuyển giao
Noidung     NVARCHAR(500)
Ngaygio     DATETIME
NguoiQL     INT            -- FK -> Taikhoan (người quản lý)
Taikhoan    NVARCHAR(50)   -- Username người tạo
Trangthai   INT            -- 1=mới, 2=đang chờ, 3=quan trọng, 4=đã xử lý
Chiphi      FLOAT
Keyword     NVARCHAR(500)  -- Từ khóa, danh sách ID trong EventKeyword
Idbatdau    INT
```

#### Bảng `ThongtinHS` — Thông tin hồ sơ pháp lý
```sql
ID                      INT IDENTITY PK
Magiaykiemdinh          NVARCHAR(50)
Magiaynhapkhau          NVARCHAR(50)
Thoihangiaynhapkhau     NVARCHAR(50)
Magiayphepluuhanh       NVARCHAR(50)
Thoihangiayphepluuhanh  NVARCHAR(50)
MagiayCO                NVARCHAR(50)
XuatxuCO                NVARCHAR(50)
MaCQ                    NVARCHAR(50)
NamsxCQ                 NVARCHAR(50)
MatokhaiHQ              NVARCHAR(50)
Packinglist             NVARCHAR(50)
Mavandon                NVARCHAR(50)
MaInvoiceNK             NVARCHAR(50)
MahoadonBH              NVARCHAR(50)
FileHS                  NVARCHAR(MAX)      -- JSON danh sách file
Thongtingoithau         NVARCHAR(MAX)
GiacoVAT                FLOAT
GiatruVAT               FLOAT
Thongtinkhauhao         NVARCHAR(MAX)
Nguonvon                NVARCHAR(50)
MaThietbi               NVARCHAR(MAX)      -- FK -> Quanlythietbi.ID
```

#### Bảng `Goithau` — Gói thầu
```sql
ID              INT IDENTITY PK
idGoithau       NVARCHAR(50)
Tengoithau      NVARCHAR(MAX)
Tinhtrang       NVARCHAR(500)
Donvitrungthau  NVARCHAR(500)
MST             NVARCHAR(200)
SoHD            NVARCHAR(50)
Giatri          FLOAT
```

#### Bảng `QuanlyMa` — Mã QR/NFC
```sql
ID          INT IDENTITY PK
IDTB        INT            -- FK -> Quanlythietbi.ID
QRCODE      NVARCHAR(50)
NFC         NVARCHAR(50)
IDNguoiSD   INT
```

#### Bảng `Quanlyfile` — File đính kèm
```sql
ID          INT IDENTITY PK
Path        NVARCHAR(500)
Mota        NVARCHAR(500)
idTB        INT            -- FK -> Quanlythietbi.ID
Ngaythang   DATE
```

#### Bảng `Quanlyvattu` — Vật tư thiết bị
```sql
ID          INT IDENTITY PK
Tenvattu    NVARCHAR(500)
Donvitinh   NVARCHAR(100)
Soluong     INT
MaTB        INT            -- FK -> Quanlythietbi.ID
```

#### Bảng `InputByDate` — Nhật ký sử dụng theo ngày
```sql
ID          INT IDENTITY PK
MaTB        NCHAR(50)      -- FK -> Quanlythietbi.MaTB
Sogio       INT            -- Số giờ sử dụng
Soca        INT            -- Số ca
InputDate   DATE
Taikhoan    INT
```

#### Bảng `LogHethong` — Log hệ thống
```sql
ID          INT IDENTITY PK
Taikhoan    INT            -- FK -> Taikhoan.ID
Noidung     NVARCHAR(MAX)
Thoigian    DATETIME
MaTB        INT
```

#### Bảng `Thongbao` — Thông báo
```sql
ID          INT PK
Noidung     NCHAR(500)
Ngaygio     DATETIME
Nguoitao    NCHAR(50)
Trangthai   INT
Tieude      NCHAR(100)
```

#### Bảng `Thongbaostatus` — Trạng thái đọc thông báo
```sql
ID          INT PK
IDthongbao  INT            -- FK -> Thongbao.ID
IDuser      INT            -- FK -> Taikhoan.ID
read_status INT
```

#### Bảng `EventKeyword` — Từ khóa sự cố
```sql
ID          INT IDENTITY PK
Keyword     NCHAR(50)
```

### 3.2 Danh mục (DM) tables
| Bảng | Mô tả |
|---|---|
| `DMLoaiTB` | Danh mục loại thiết bị |
| `DMTrangthai` | Danh mục trạng thái (1=đang hoạt động, 2=hỏng, 3=chưa dùng...) |
| `DanhmucDV` | Danh mục loại đơn vị |
| `DanhmucLoaiNS` | Danh mục loại nhân sự |
| `DanhmucHoso` | Danh mục loại hồ sơ |
| `DMTacvu` | Danh mục tác vụ (loại sự kiện gửi yêu cầu) |
| `Country` | Danh sách quốc gia |

### 3.3 Views quan trọng
| View | Mô tả |
|---|---|
| `v_report` | Join Quanlythietbi + DMLoaiTB -> báo cáo tổng hợp |
| `v_PushNhansu` | View nhân sự + tài khoản để push notification |
| `v_Logthietbi` | Join LogThietbi + Quanlythietbi -> log kèm thông tin TB |

### 3.4 Kho vật tư/thuốc (ít dùng - có thể phase 2)
| Bảng | Mô tả |
|---|---|
| `DMthuocvattu` | Danh mục thuốc/vật tư |
| `Nhapthuocvattu` | Nhập kho |
| `Xuatthuocvattu` | Xuất kho |
| `Solothuocvattu` | Số lô |
| `Nhacungcap` | Nhà cung cấp |

---

## 4. PHÂN TÍCH CHỨC NĂNG (Mapping .aspx → API)

### MODULE 1: XÁC THỰC (Authentication)
**File cũ:** `login.aspx.vb`, `doimatkhau.aspx.vb`, `quenmatkhau.aspx.vb`

**Chức năng:**
- Đăng nhập bằng username/password
- Đổi mật khẩu
- Gửi email thông báo hết hạn kiểm định khi login
- Đa ngôn ngữ (vi/en) — hiện chỉ có Vi

**API mới:**
```
POST   /api/auth/login          → LoginRequest { username, password } → { token, user }
POST   /api/auth/change-password → ChangePasswordRequest
POST   /api/auth/forgot-password → ForgotPasswordRequest { email }
```

---

### MODULE 2: QUẢN LÝ THIẾT BỊ (Device Management) — CORE
**File cũ:** `Themdulieu.aspx.vb`, `edit.aspx.vb`, `view.aspx.vb`, `Timkiem.aspx.vb`, `import.aspx.vb`

**Chức năng chi tiết:**
- **Thêm thiết bị mới** (Themdulieu.aspx): Nhập đầy đủ thông tin thiết bị, upload ảnh đại diện
- **Xem chi tiết thiết bị** (view.aspx): Tab thông tin chính, hồ sơ, vật tư, lịch sử log, file đính kèm
- **Chỉnh sửa thiết bị** (edit.aspx): 
  - Chỉnh sửa thông tin cơ bản
  - Upload/thay ảnh đại diện
  - Gắn/thay QR code cho thiết bị
  - Xem danh sách thiết bị con (Thietbichinh)
- **Tìm kiếm** (Timkiem.aspx): Tìm kiếm đa tiêu chí (tên, mã, khoa, trạng thái, loại, thời gian...)
- **Import Excel** (import.aspx): Upload file Excel để nhập hàng loạt thiết bị

**API mới:**
```
GET    /api/devices                         → Danh sách (phân trang, filter, search)
GET    /api/devices/{id}                    → Chi tiết thiết bị
POST   /api/devices                         → Thêm mới
PUT    /api/devices/{id}                    → Cập nhật
DELETE /api/devices/{id}                    → Xóa
GET    /api/devices/{id}/children           → Thiết bị con
GET    /api/devices/{id}/logs               → Lịch sử log
GET    /api/devices/{id}/files              → File đính kèm
GET    /api/devices/{id}/accessories        → Vật tư đi kèm
POST   /api/devices/import                  → Import Excel
GET    /api/devices/search                  → Tìm kiếm nâng cao
POST   /api/devices/{id}/image              → Upload ảnh
GET    /api/devices/{id}/qrcode             → Lấy QR code
POST   /api/devices/{id}/qrcode             → Gắn QR code mới
PUT    /api/devices/{id}/qrcode             → Cập nhật QR code
```

**Query params cho GET /api/devices:**
- `page`, `pageSize`
- `search` (tên/mã)
- `donviId`, `loaiId`, `trangthaiId`
- `tuNgay`, `denNgay` (ngày tiếp nhận)
- `sapHetHanKiemDinh` (bool — sắp hết hạn kiểm định)
- `sortBy`, `sortDir`

---

### MODULE 3: HỒ SƠ PHÁP LÝ (Legal Documents)
**File cũ:** `hoso.aspx.vb`, `thongtinhs.aspx.vb`

**Chức năng:**
- Quản lý hồ sơ khai thác (giấy kiểm định, nhập khẩu, lưu hành, C/O, khai HQ...)
- Upload và quản lý file hồ sơ
- Thông tin gói thầu liên quan

**API mới:**
```
GET    /api/devices/{id}/legal-documents    → Hồ sơ pháp lý
PUT    /api/devices/{id}/legal-documents    → Cập nhật hồ sơ
POST   /api/devices/{id}/legal-documents/files → Upload file
DELETE /api/legal-documents/files/{fileId}  → Xóa file
```

---

### MODULE 4: NHẬT KÝ / SỰ CỐ (Event Log)
**File cũ:** `guiyc.aspx.vb`, `xuly.aspx.vb`, `NhatkyTB.aspx.vb`, `log.aspx.vb`, `loga.aspx.vb`

**Chức năng chi tiết:**
- **Báo cáo sự cố** (guiyc.aspx):
  - Gửi yêu cầu: báo hỏng, đề nghị kiểm tra, yêu cầu khác
  - Chọn tác vụ từ DMTacvu
  - Nhập từ khóa (EventKeyword)
  - Upload ảnh sự cố
  - Chọn nhân sự nhận thông báo → Push notification + Email
- **Xử lý sự cố** (xuly.aspx):
  - Cập nhật trạng thái xử lý
  - Nhập chi phí xử lý
  - Ghi nội dung xử lý
  - Push notification kết quả
- **Nhật ký sử dụng** (NhatkyTB.aspx): Ghi nhật ký theo ngày (số giờ, số ca)
- **Xem log** (log.aspx, loga.aspx): Xem lịch sử tất cả sự kiện

**API mới:**
```
GET    /api/devices/{id}/events             → Danh sách sự kiện
POST   /api/devices/{id}/events             → Báo cáo sự cố/gửi yêu cầu
PUT    /api/events/{id}                     → Cập nhật trạng thái xử lý
GET    /api/events                          → Tất cả sự kiện (admin)
POST   /api/devices/{id}/events/{eventId}/files → Upload file sự cố

GET    /api/devices/{id}/usage-logs         → Nhật ký sử dụng
POST   /api/devices/{id}/usage-logs         → Thêm nhật ký ngày

GET    /api/events/keywords                 → Danh sách EventKeyword
POST   /api/events/keywords                 → Thêm keyword
```

---

### MODULE 5: BÁO CÁO (Reports)
**File cũ:** `baocao.aspx.vb`, `baocaoTS.aspx.vb`, `baocaokho.aspx.vb`

**Chức năng:**
- **Báo cáo thiết bị** (baocao.aspx): 
  - Danh sách thiết bị theo khoa/loại/trạng thái
  - Xuất Excel
  - Thống kê chi phí, nguyên giá, khấu hao
  - Filter đa điều kiện
- **Báo cáo tài sản** (baocaoTS.aspx): Báo cáo tài sản, nguồn vốn, khấu hao
- **Báo cáo kho** (baocaokho.aspx): Báo cáo tồn kho vật tư

**API mới:**
```
GET    /api/reports/devices                 → Báo cáo danh sách TB (params: filter)
GET    /api/reports/devices/export          → Xuất Excel báo cáo TB
GET    /api/reports/assets                  → Báo cáo tài sản
GET    /api/reports/assets/export           → Xuất Excel tài sản
GET    /api/reports/warehouse               → Báo cáo kho
GET    /api/reports/maintenance             → Báo cáo bảo trì
```

---

### MODULE 6: DASHBOARD
**File cũ:** `Dashboard.aspx.vb`

**Chức năng:**
- Biểu đồ cột: Số TB + Tổng tiền theo Khoa
- Biểu đồ đường: Số TB theo khoa
- Biểu đồ tròn: Phân bổ đầu tư theo khoa (triệu VND)
- Biểu đồ tròn: Phân bổ số lượng TB theo khoa
- Bảng tổng hợp số liệu

**API mới:**
```
GET    /api/dashboard/summary               → { totalDevices, totalValue, ... }
GET    /api/dashboard/by-department         → Thống kê theo khoa/đơn vị
GET    /api/dashboard/by-status             → Thống kê theo trạng thái
GET    /api/dashboard/by-type               → Thống kê theo loại TB
GET    /api/dashboard/expiring-soon         → TB sắp hết hạn kiểm định/bảo hành
```

---

### MODULE 7: KÝ SỐ PDF (Digital Signature)
**File cũ:** `sign.aspx.vb` (~884 dòng)

**Chức năng:**
- Upload file PDF
- Chọn vị trí ký trên PDF (tọa độ x,y,w,h, page)
- Ký số bằng certificate (X.509)
- Tích hợp với Firebase để notify
- Sử dụng iTextSharp để xử lý PDF

**API mới:**
```
POST   /api/documents/upload                → Upload PDF để ký
POST   /api/documents/{docId}/sign         → Ký số PDF
GET    /api/documents/{docId}              → Lấy file PDF đã ký
GET    /api/documents/certificates         → Danh sách certificates
```

> **Lưu ý:** Module này phức tạp — dùng iTextSharp 7 / PdfSharp trong .NET 7. Frontend cần PDF viewer + signature pad.

---

### MODULE 8: QUẢN LÝ NHÂN SỰ (Personnel)
**File cũ:** `quanlynhansu.aspx.vb`, `thongtinnhansu.aspx.vb`

**Chức năng:**
- CRUD nhân sự
- Gán nhân sự vào đơn vị
- Xem thông tin nhân sự

**API mới:**
```
GET    /api/personnel                       → Danh sách nhân sự
GET    /api/personnel/{id}                  → Chi tiết
POST   /api/personnel                       → Thêm mới
PUT    /api/personnel/{id}                  → Cập nhật
DELETE /api/personnel/{id}                  → Xóa
```

---

### MODULE 9: QUẢN LÝ TÀI KHOẢN (User Management)
**File cũ:** `taikhoan.aspx.vb`

**Chức năng:**
- CRUD tài khoản
- Phân quyền: Admin (LoaiTK=1), Kỹ thuật (LoaiTK=2), Người dùng (LoaiTK=3)
- Gán tài khoản vào đơn vị (MaDV - có thể nhiều DV)

**API mới:**
```
GET    /api/users                           → Danh sách (admin only)
GET    /api/users/{id}                      → Chi tiết
POST   /api/users                           → Tạo tài khoản
PUT    /api/users/{id}                      → Cập nhật
DELETE /api/users/{id}                      → Xóa
PUT    /api/users/{id}/password             → Đặt lại mật khẩu
GET    /api/users/me                        → Profile hiện tại
PUT    /api/users/me                        → Cập nhật profile
```

---

### MODULE 10: QUẢN LÝ ĐƠN VỊ (Organization Units)
**File cũ:** `donvi.aspx.vb`, `khoaphong.aspx.vb`

**Chức năng:**
- CRUD đơn vị: Khoa, phòng, nhà cung cấp, đơn vị bảo trì
- Phân loại đơn vị theo DanhmucDV

**API mới:**
```
GET    /api/organizations                   → Danh sách đơn vị
GET    /api/organizations/{id}              → Chi tiết
POST   /api/organizations                   → Thêm mới
PUT    /api/organizations/{id}              → Cập nhật
DELETE /api/organizations/{id}              → Xóa
GET    /api/organizations/types             → Danh mục loại đơn vị (DanhmucDV)
```

---

### MODULE 11: GÓI THẦU (Bidding Packages)
**File cũ:** `goithau.aspx.vb`, `themmoigoithau.aspx.vb`, `chinhsuagoithau.aspx.vb`

**Chức năng:**
- CRUD gói thầu
- Gán thiết bị vào gói thầu

**API mới:**
```
GET    /api/bidding-packages                → Danh sách gói thầu
GET    /api/bidding-packages/{id}           → Chi tiết
POST   /api/bidding-packages                → Thêm mới
PUT    /api/bidding-packages/{id}           → Cập nhật
DELETE /api/bidding-packages/{id}           → Xóa
GET    /api/bidding-packages/{id}/devices   → TB thuộc gói thầu
```

---

### MODULE 12: VẬT TƯ THIẾT BỊ (Accessories)
**File cũ:** `vattu.aspx.vb`

**Chức năng:**
- Quản lý vật tư đi kèm thiết bị
- Nhập/xuất vật tư (kho)

**API mới:**
```
GET    /api/devices/{id}/accessories        → Danh sách vật tư
POST   /api/devices/{id}/accessories        → Thêm vật tư
PUT    /api/accessories/{id}                → Cập nhật
DELETE /api/accessories/{id}                → Xóa
```

---

### MODULE 13: SCAN QR / CAMERA
**File cũ:** `scanqr.aspx.vb`, `Camera.aspx.vb`, `CaptureNhatky.aspx.vb`

**Chức năng:**
- Scan QR code bằng camera → tìm thiết bị
- Chụp ảnh từ camera → lưu vào nhật ký hoặc sự cố

**Frontend xử lý:** Dùng `react-webcam` hoặc `@zxing/browser` để scan QR trong browser. Không cần API riêng, chỉ cần gọi API search bằng QR code value.

---

### MODULE 14: THÔNG BÁO (Notifications)
**File cũ:** `thongbao.aspx.vb`

**Chức năng:**
- Xem danh sách thông báo
- Đánh dấu đã đọc

**API mới:**
```
GET    /api/notifications                   → Danh sách thông báo
PUT    /api/notifications/{id}/read         → Đánh dấu đã đọc
PUT    /api/notifications/read-all          → Đánh dấu tất cả đã đọc
GET    /api/notifications/unread-count      → Số lượng chưa đọc
```

---

### MODULE 15: CHUYỂN GIAO THIẾT BỊ (Transfer)
**File cũ:** `chuyengiao.aspx.vb`

**Chức năng:**
- Chuyển giao thiết bị giữa các khoa/đơn vị
- Ghi log chuyển giao

**API mới:**
```
POST   /api/devices/{id}/transfer           → Chuyển giao
GET    /api/devices/{id}/transfer-history   → Lịch sử chuyển giao
```

---

### MODULE 16: DANH MỤC HỆ THỐNG (Master Data)
**File cũ:** Các select box trong nhiều trang

**API mới:**
```
GET    /api/master/device-types             → DMLoaiTB
GET    /api/master/status                   → DMTrangthai  
GET    /api/master/org-types                → DanhmucDV
GET    /api/master/personnel-types          → DanhmucLoaiNS
GET    /api/master/document-types           → DanhmucHoso
GET    /api/master/task-types               → DMTacvu
GET    /api/master/countries                → Country
GET    /api/master/event-keywords           → EventKeyword

POST/PUT/DELETE cho từng danh mục (admin only)
```

---

## 5. CẤU TRÚC PROJECT .NET 7 API

```
QLTBYT.API/
├── Program.cs
├── appsettings.json
├── appsettings.Development.json
│
├── Controllers/
│   ├── AuthController.cs
│   ├── DevicesController.cs
│   ├── DeviceEventsController.cs
│   ├── DeviceFilesController.cs
│   ├── DeviceAccessoriesController.cs
│   ├── DeviceUsageLogsController.cs
│   ├── LegalDocumentsController.cs
│   ├── OrganizationsController.cs
│   ├── PersonnelController.cs
│   ├── UsersController.cs
│   ├── BiddingPackagesController.cs
│   ├── NotificationsController.cs
│   ├── ReportsController.cs
│   ├── DashboardController.cs
│   ├── MasterDataController.cs
│   └── DocumentSignController.cs
│
├── Models/
│   ├── Domain/           ← EF Core entities (1-1 mapping với DB tables)
│   │   ├── Device.cs
│   │   ├── UserAccount.cs
│   │   ├── Organization.cs
│   │   ├── Personnel.cs
│   │   ├── DeviceLog.cs
│   │   ├── DeviceFile.cs
│   │   ├── DeviceAccessory.cs
│   │   ├── LegalDocument.cs
│   │   ├── BiddingPackage.cs
│   │   ├── Notification.cs
│   │   ├── DeviceCode.cs     ← QuanlyMa
│   │   ├── UsageLog.cs       ← InputByDate
│   │   ├── SystemLog.cs      ← LogHethong
│   │   └── ... (DM tables)
│   │
│   └── DTOs/             ← Request/Response objects
│       ├── Auth/
│       │   ├── LoginRequest.cs
│       │   ├── LoginResponse.cs
│       │   └── ChangePasswordRequest.cs
│       ├── Device/
│       │   ├── DeviceListDto.cs
│       │   ├── DeviceDetailDto.cs
│       │   ├── CreateDeviceRequest.cs
│       │   └── UpdateDeviceRequest.cs
│       ├── Common/
│       │   ├── PagedResult.cs
│       │   └── ApiResponse.cs
│       └── ...
│
├── Services/
│   ├── IAuthService.cs / AuthService.cs
│   ├── IDeviceService.cs / DeviceService.cs
│   ├── IEmailService.cs / EmailService.cs
│   ├── IPushNotificationService.cs / FirebasePushService.cs
│   ├── IReportService.cs / ReportService.cs
│   ├── IFileStorageService.cs / LocalFileStorageService.cs
│   ├── IQRCodeService.cs / QRCodeService.cs
│   └── IPdfSignService.cs / PdfSignService.cs
│
├── Data/
│   ├── AppDbContext.cs
│   └── Configurations/   ← EF Core Fluent API configs
│
├── Middleware/
│   ├── ErrorHandlingMiddleware.cs
│   └── RequestLoggingMiddleware.cs
│
├── Helpers/
│   ├── JwtHelper.cs
│   ├── PasswordHasher.cs
│   └── ExcelHelper.cs
│
└── Infrastructure/
    └── BackgroundServices/
        └── DeviceExpiryNotificationJob.cs  ← Thay login.aspx email logic
```

---

## 6. CẤU TRÚC PROJECT REACT

```
qltbyt-frontend/
├── public/
├── src/
│   ├── main.tsx
│   ├── App.tsx
│   │
│   ├── api/                      ← API client (axios instances)
│   │   ├── axiosClient.ts
│   │   ├── authApi.ts
│   │   ├── deviceApi.ts
│   │   ├── reportApi.ts
│   │   └── ...
│   │
│   ├── components/               ← Shared/reusable components
│   │   ├── Layout/
│   │   │   ├── MainLayout.tsx
│   │   │   ├── Sidebar.tsx
│   │   │   └── Header.tsx
│   │   ├── Common/
│   │   │   ├── DataTable.tsx     ← Thay ASPxGrid
│   │   │   ├── SearchBar.tsx
│   │   │   ├── FileUpload.tsx    ← Thay ASPxUpload
│   │   │   ├── QRScanner.tsx
│   │   │   ├── QRCodeDisplay.tsx
│   │   │   └── NotificationBell.tsx
│   │   └── Charts/
│   │       ├── BarChart.tsx
│   │       ├── PieChart.tsx
│   │       └── LineChart.tsx
│   │
│   ├── pages/                    ← Page components (1 page = 1 aspx cũ)
│   │   ├── Login/
│   │   ├── Dashboard/
│   │   ├── Devices/
│   │   │   ├── DeviceList.tsx
│   │   │   ├── DeviceCreate.tsx
│   │   │   ├── DeviceEdit.tsx
│   │   │   ├── DeviceDetail.tsx
│   │   │   └── DeviceImport.tsx
│   │   ├── Events/              ← Log/Sự cố
│   │   │   ├── EventList.tsx
│   │   │   ├── ReportIncident.tsx  ← guiyc.aspx
│   │   │   └── ResolveEvent.tsx    ← xuly.aspx
│   │   ├── Reports/
│   │   ├── Organizations/
│   │   ├── Personnel/
│   │   ├── Users/
│   │   ├── BiddingPackages/
│   │   ├── Notifications/
│   │   └── Settings/            ← Master data management
│   │
│   ├── hooks/                    ← Custom hooks
│   │   ├── useAuth.ts
│   │   ├── useDevices.ts
│   │   └── useNotifications.ts
│   │
│   ├── store/                    ← Zustand stores
│   │   ├── authStore.ts
│   │   └── notificationStore.ts
│   │
│   ├── types/                    ← TypeScript interfaces
│   │   ├── device.ts
│   │   ├── user.ts
│   │   └── ...
│   │
│   └── utils/
│       ├── formatters.ts
│       └── validators.ts
│
├── package.json
├── vite.config.ts
└── tsconfig.json
```

---

## 7. PHÂN QUYỀN (Authorization)

### Role mapping (từ `LoaiTK` trong Taikhoan):
| LoaiTK | Role | Quyền |
|---|---|---|
| 1 | Admin | Toàn quyền: CRUD tất cả, quản lý user, danh mục |
| 2 | Kỹ thuật (Technician) | CRUD thiết bị, xử lý sự cố, xem báo cáo |
| 3 | Người dùng (User) | Xem thiết bị, báo cáo sự cố, xem nhật ký |

### Authorization policies trong .NET 7:
```csharp
// Program.cs
builder.Services.AddAuthorization(options => {
    options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
    options.AddPolicy("TechnicianAndAbove", p => p.RequireRole("Admin", "Technician"));
    options.AddPolicy("AllUsers", p => p.RequireRole("Admin", "Technician", "User"));
});
```

### Lọc dữ liệu theo đơn vị (MaDV):
- User thường chỉ thấy thiết bị của đơn vị mình (MaDV)
- Admin/Technician thấy tất cả
- Logic này cần implement trong Service layer

---

## 8. TÍNH NĂNG ĐẶC BIỆT CẦN LƯU Ý

### 8.1 Email Tự động — Hết hạn kiểm định
**Cũ:** Chạy trong `login_Load` event — không tốt vì phụ thuộc vào login  
**Mới:** Background Service (`IHostedService`) hoặc Hangfire scheduler
```csharp
// Chạy hàng ngày lúc 6:00 sáng
// Query TB: hankiemdinh < (NOW + 30 days) AND hankiemdinh > NOW AND MaKT = '0'
// Gửi email cho Nguoiquanly
// Update MaKT = '1'
```

### 8.2 Push Notification (Firebase FCM)
**Cũ:** Dùng Legacy FCM API (deprecated)  
**Mới:** Firebase Admin SDK v2+ với HTTP v1 API
```csharp
// NuGet: FirebaseAdmin
// Gửi notification khi:
// - Có sự cố mới (guiyc)
// - Sự cố được xử lý (xuly)
// - Thiết bị được chuyển giao
```

### 8.3 QR Code
**Cũ:** Generate QR trên server, trả về base64 image  
**Mới:** Generate QR trên Frontend với `qrcode.react`  
**Backend:** Chỉ cần store và retrieve QR code text

### 8.4 Upload File
**Cũ:** DevExpress ASPxUpload, lưu vào folder `~/TAILIEU/{IDTB}/...`  
**Mới:**
- Frontend: `react-dropzone` hoặc Ant Design Upload
- Backend: Multipart/form-data endpoint
- Storage: Local file system (giữ nguyên structure) hoặc upgrade lên Azure Blob/MinIO

**Cấu trúc thư mục file hiện tại:**
```
TAILIEU/
└── {IDTB}/
    ├── Hinh_anh_su_co/    ← Ảnh báo cáo sự cố
    └── BAO_CAO_SU_CO/     ← File báo cáo sự cố
```

### 8.5 PDF Ký số (sign.aspx)
**Cũ:** iTextSharp + X.509 certificate  
**Mới:** iText7 (cùng vendor, API mới hơn) hoặc PdfSharp + BouncyCastle  
Frontend cần: PDF viewer (react-pdf) + UI để chọn vị trí ký

### 8.6 Import Excel (import.aspx)
**Cũ:** DevExpress SpreadsheetControl  
**Mới:** NPOI hoặc EPPlus trong .NET 7  
Frontend: Upload Excel → Backend parse → Validate → Insert

---

## 9. CẤU HÌNH & BIẾN MÔI TRƯỜNG

### appsettings.json (Backend):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=CUONG-PC\\SQLSERVER;Database=db_QLTBYT;User Id=sa;Password=zaq1ZAQ!;TrustServerCertificate=True"
  },
  "JwtSettings": {
    "SecretKey": "your-very-long-secret-key-at-least-32-chars",
    "Issuer": "QLTBYT-API",
    "Audience": "QLTBYT-Frontend",
    "ExpiryHours": 24
  },
  "EmailSettings": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "Username": "appungbuou@gmail.com",
    "Password": "prca trhb lsbc nmdf",
    "FromName": "Hệ thống QLTBYT"
  },
  "Firebase": {
    "ProjectId": "pushnotifcationios",
    "ServiceAccountKeyPath": "firebase-service-account.json"
  },
  "FileStorage": {
    "BasePath": "D:/DVC/PROJECT/QLTBTYT/QLTBYT/QLTBYT",
    "MaxFileSizeMB": 50
  },
  "NotificationSettings": {
    "DaysBeforeExpiryAlert": 30
  }
}
```

### .env (Frontend):
```
VITE_API_BASE_URL=http://localhost:5000/api
VITE_APP_TITLE=Quản Lý Thiết Bị Y Tế
```

---

## 10. SECURITY IMPROVEMENTS

| Vấn đề cũ | Giải pháp mới |
|---|---|
| Mật khẩu plain text | BCrypt hash (`BCrypt.Net-Next`) |
| Không có JWT | ASP.NET Core JWT Bearer |
| SQL Injection trong `service.vb` | EF Core ORM + parameterized queries |
| Không có CORS | Cấu hình CORS middleware cho React origin |
| Session-based auth | Stateless JWT |
| Hardcode connection string trong code | appsettings.json + environment variables |
| Firebase key trong code | appsettings.json |
| Email password trong code | appsettings.json / Azure Key Vault |

---

## 11. KẾ HOẠCH TRIỂN KHAI (Phased Approach)

### Phase 1 — Core (4-6 tuần)
- [ ] Thiết lập project structure (React + .NET 7)
- [ ] Authentication (JWT login/logout)
- [ ] Module Quản lý Thiết bị (CRUD)
- [ ] Module Tìm kiếm
- [ ] Upload ảnh + file
- [ ] Dashboard cơ bản

### Phase 2 — Operations (3-4 tuần)
- [ ] Module Log/Sự cố (báo cáo + xử lý)
- [ ] Module Nhật ký sử dụng
- [ ] Push notification (Firebase)
- [ ] Email tự động (Background Service)
- [ ] Quản lý tài khoản + nhân sự

### Phase 3 — Advanced (3-4 tuần)
- [ ] Module Báo cáo + Export Excel
- [ ] Module Gói thầu + Hồ sơ pháp lý
- [ ] Scan QR Code
- [ ] Import Excel hàng loạt
- [ ] Phân quyền đầy đủ

### Phase 4 — Premium Features (2-3 tuần)
- [ ] Ký số PDF
- [ ] Module Kho vật tư/thuốc
- [ ] Responsive mobile
- [ ] Performance optimization

---

## 12. PACKAGES / DEPENDENCIES

### Backend (.NET 7 NuGet):
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="7.x" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="7.x" />
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="7.x" />
<PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
<PackageReference Include="AutoMapper.Extensions.Microsoft.DependencyInjection" Version="12.x" />
<PackageReference Include="FluentValidation.AspNetCore" Version="11.x" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.x" />
<PackageReference Include="MailKit" Version="4.x" />
<PackageReference Include="FirebaseAdmin" Version="2.x" />
<PackageReference Include="QRCoder" Version="1.4.3" />
<PackageReference Include="EPPlus" Version="7.x" />
<PackageReference Include="itext7" Version="8.x" />
<PackageReference Include="Serilog.AspNetCore" Version="7.x" />
<PackageReference Include="NPOI" Version="2.x" />
```

### Frontend (npm):
```json
{
  "dependencies": {
    "react": "^18.x",
    "react-dom": "^18.x",
    "react-router-dom": "^6.x",
    "axios": "^1.x",
    "@tanstack/react-query": "^5.x",
    "zustand": "^4.x",
    "antd": "^5.x",
    "@ant-design/icons": "^5.x",
    "recharts": "^2.x",
    "react-dropzone": "^14.x",
    "qrcode.react": "^3.x",
    "@zxing/browser": "^0.1.x",
    "react-webcam": "^7.x",
    "xlsx": "^0.18.x",
    "dayjs": "^1.x",
    "react-pdf": "^7.x"
  },
  "devDependencies": {
    "typescript": "^5.x",
    "@types/react": "^18.x",
    "vite": "^5.x",
    "@vitejs/plugin-react": "^4.x"
  }
}
```

---

## 13. MAPPING ASPX → REACT PAGES

| File ASPX cũ | React Page mới | API Endpoint |
|---|---|---|
| `login.aspx` | `/login` | `POST /api/auth/login` |
| `Dashboard.aspx` | `/dashboard` | `GET /api/dashboard/*` |
| `Themdulieu.aspx` | `/devices/create` | `POST /api/devices` |
| `edit.aspx` | `/devices/:id/edit` | `PUT /api/devices/:id` |
| `view.aspx` | `/devices/:id` | `GET /api/devices/:id` |
| `Timkiem.aspx` | `/devices` (+ filter) | `GET /api/devices` |
| `import.aspx` | `/devices/import` | `POST /api/devices/import` |
| `guiyc.aspx` | `/devices/:id/report-incident` | `POST /api/devices/:id/events` |
| `xuly.aspx` | `/events/:id/resolve` | `PUT /api/events/:id` |
| `NhatkyTB.aspx` | `/devices/:id/usage-log` | `POST /api/devices/:id/usage-logs` |
| `log.aspx` | `/events` | `GET /api/events` |
| `loga.aspx` | `/events` (admin view) | `GET /api/events?admin=true` |
| `baocao.aspx` | `/reports/devices` | `GET /api/reports/devices` |
| `baocaoTS.aspx` | `/reports/assets` | `GET /api/reports/assets` |
| `baocaokho.aspx` | `/reports/warehouse` | `GET /api/reports/warehouse` |
| `donvi.aspx` | `/organizations` | `GET/POST /api/organizations` |
| `khoaphong.aspx` | `/organizations` (tab) | - |
| `quanlynhansu.aspx` | `/personnel` | `GET /api/personnel` |
| `taikhoan.aspx` | `/users` | `GET /api/users` |
| `goithau.aspx` | `/bidding-packages` | `GET /api/bidding-packages` |
| `vattu.aspx` | `/devices/:id` (tab Vật tư) | `GET /api/devices/:id/accessories` |
| `hoso.aspx` | `/devices/:id` (tab Hồ sơ) | `GET /api/devices/:id/legal-documents` |
| `sign.aspx` | `/sign-document` | `POST /api/documents/sign` |
| `thongbao.aspx` | Header notification | `GET /api/notifications` |
| `chuyengiao.aspx` | Dialog trong Device Detail | `POST /api/devices/:id/transfer` |
| `scanqr.aspx` | Modal QR Scanner | Frontend-only với camera |
| `doimatkhau.aspx` | `/profile/change-password` | `PUT /api/users/me/password` |
| `history.aspx` | `/devices/:id` (tab History) | `GET /api/devices/:id/logs` |

---

## 14. ĐIỂM CẦN ĐẶC BIỆT CHÚ Ý KHI CODE

### 14.1 Trạng thái thiết bị (DMTrangthai)
```
1 = Đang hoạt động
2 = Không hoạt động / Hỏng
3 = Chưa sử dụng
8 = Không hoạt động (variant khác)
```
> **Cần xác nhận lại mapping đầy đủ từ database thực tế.**

### 14.2 Loại log sự kiện (LogThietbi.Loai)
```
1 = Báo hỏng → Thiết bị đã ổn
2 = Đề nghị kiểm tra / Báo hỏng (duplicate?)
3 = Yêu cầu khác / Đề nghị kiểm tra
4 = Yêu cầu khác / Chuyển giao TBYT
5 = Đang xử lý
6 = Báo hỏng (variant)
```
> **Chú ý:** Comment trong `LogThietbi` có mâu thuẫn với code trong `xuly.aspx`. Cần review business logic.

### 14.3 Trạng thái log xử lý (LogThietbi.Trangthai)
```
1 = Mới
2 = Đang chờ xử lý
3 = Quan trọng
4 = Đã xử lý xong
```

### 14.4 Loại tài khoản (Taikhoan.LoaiTK)
```
< 2 = Admin (xem v_PushNhansu where LoaiTK < 2)
< 3 = Kỹ thuật trở lên (xem v_PushNhansu where LoaiTK < 3)
```

### 14.5 Field MaDV trong Taikhoan
`MaDV` trong bảng `Taikhoan` có thể chứa nhiều ID đơn vị, ngăn cách bằng `;`.  
**Mới:** Nên tạo bảng `UserOrganization` (many-to-many) thay vì store JSON trong field.

### 14.6 Xử lý legacy data — MaKT
Field `MaKT` trong `Quanlythietbi` dùng để track đã gửi email kiểm định hay chưa.  
Backend mới nên reset MaKT hàng năm hoặc dùng date-based tracking thay vì flag đơn giản.

---

## 15. CURSOR PROMPTS GỢI Ý

Dưới đây là các prompt mẫu để dùng với Cursor khi implement từng phần:

### Tạo Backend Project:
```
Tạo .NET 7 Web API project tên QLTBYT.API với:
- Entity Framework Core 7 kết nối SQL Server
- JWT Authentication
- Swagger UI
- CORS cho http://localhost:5173 (Vite React dev)
- Folder structure theo Clean Architecture: Controllers, Models/Domain, Models/DTOs, Services, Data
- Global error handling middleware
- Serilog logging
Connection string: Server=CUONG-PC\SQLSERVER;Database=db_QLTBYT;User Id=sa;Password=zaq1ZAQ!;TrustServerCertificate=True
```

### Tạo Entity từ Schema:
```
Tạo EF Core entity class cho bảng Quanlythietbi với đầy đủ properties theo schema:
[dán schema từ mục 3.1]
Tạo kèm: DbContext configuration, DTO (DeviceListDto, DeviceDetailDto, CreateDeviceRequest, UpdateDeviceRequest)
Dùng AutoMapper để map giữa entity và DTO
```

### Tạo React Frontend:
```
Tạo Vite React TypeScript project tên qltbyt-frontend với:
- React Router v6 
- Ant Design v5
- Axios với JWT interceptor (tự động thêm Bearer token, handle 401)
- React Query v5 cho data fetching
- Zustand cho auth state
- Layout với Sidebar navigation có các menu: Dashboard, Thiết bị, Sự cố, Báo cáo, Quản lý (DV/NS/User), Cài đặt
Tất cả text tiếng Việt
```

---

*Tài liệu này được tạo tự động bằng phân tích codebase QLTBYT ngày 03/08/2026*  
*Phiên bản: 1.0 — Cần review trước khi implement*
