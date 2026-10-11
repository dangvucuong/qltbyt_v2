# Bản đồ chức năng: QLTBYT cũ (WebForms) → QLTBYT mới (React + .NET API)

Tài liệu này giúp đối chiếu **mỗi chức năng trên giao diện dự án mới** với **trang / luồng tương ứng trong dự án cũ** (`D:\DVC\PROJECT\QLTBTYT\QLTBYT\QLTBYT`).

> **Lưu ý kiến trúc**
>
> - **Cũ:** ASP.NET WebForms (`.aspx` + VB.NET) + DevExpress, session phía server.
> - **Mới:** React SPA (menu sidebar) gọi REST API (.NET 7) bằng JWT.
> - Nhiều trang cũ tách riêng (ví dụ `guiyc.aspx`, `xuly.aspx`) được **gộp vào modal / tab** trên một trang mới — không phải mất chức năng.
> - Database `db_QLTBYT` **giữ nguyên schema**; backend map entity sang DTO.

---

## 1. Menu nhanh (sidebar mới ↔ menu cũ)

| Menu mới (sidebar) | Route mới | Tương ứng cũ (chính) | Ghi chú ngắn |
|---|---|---|---|
| **Tổng quan** | `/dashboard` | `Dashboard.aspx` | Biểu đồ + thống kê nhanh |
| **Sự cố / yêu cầu** | `/events` | `log.aspx`, `loga.aspx`, `xuly.aspx` | Danh sách + xử lý + Excel + từ khóa |
| **Thiết bị** | `/devices` | `Timkiem.aspx` (+ liên kết sang view/edit/thêm) | Hub quản lý thiết bị |
| **Quét QR** | `/scan-qr` | `scanqr.aspx` | Quét mã QR bằng camera |
| **Kiểm kê tài sản** | `/asset-inventory` | Một phần logic trong `baocaoTS.aspx` / báo cáo TS | Lọc + xuất kiểm kê |
| **Nhật ký sử dụng** | `/usage-logs` | `NhatkyTB.aspx` (và liên quan `InputByDate`) | Nhập/xem nhật ký theo mã TB |
| **Báo cáo** | `/reports` | `baocao.aspx`, `baocaoTS.aspx`, `baocaokho.aspx` | Nhiều tab gộp chung |
| **Gói thầu** | `/bidding` | `goithau.aspx`, `themmoigoithau.aspx`, `chinhsuagoithau.aspx` | CRUD gói thầu |
| **Hồ sơ pháp lý** | `/legal-documents` | `hoso.aspx`, `thongtinhs.aspx` | Hồ sơ pháp lý thiết bị |
| **Kho TVT** | `/warehouse` | `Nhaphanghoa.aspx`, `Xuathanghoa.aspx`, `baocaokho.aspx` | Nhập/xuất + báo cáo kho |
| **Ký số PDF** | `/sign-document` | `sign.aspx` | Ký số file PDF |
| **Nhân sự** | `/personnel` | `quanlynhansu.aspx`, `thongtinnhansu.aspx` | Quản lý nhân sự |
| **Đơn vị** | `/organizations` | `donvi.aspx`, `khoaphong.aspx` | Đơn vị / khoa phòng |
| **Danh mục** | `/admin/master` | Các trang/master data rải rác trong cũ | Loại TB, trạng thái, từ khóa… |
| **Nhật ký hệ thống** | `/admin/system-logs` | Bảng `LogHethong` (ít/không có UI riêng rõ ràng) | Audit thao tác hệ thống |
| **Tài khoản** | `/users` | `taikhoan.aspx` | Quản lý user |
| *(Header)* Thông báo | `/notifications` | `thongbao.aspx` | Chuông + trang danh sách |
| *(Header)* Hồ sơ | `/profile` | `doimatkhau.aspx` | Đổi mật khẩu / thông tin |

---

## 2. Đăng nhập & tài khoản

### 2.1. Đăng nhập

| Mới | Cũ |
|---|---|
| `/login` → `LoginPage` | `login.aspx` |
| API: `POST /api/auth/login` | Kiểm tra bảng `Taikhoan`, tạo Session |

**Giải thích:** Cũ dùng Session WebForms. Mới trả **JWT Bearer**; frontend lưu token (Zustand) và gửi kèm mọi request. Phân quyền theo `LoaiTK` → role `Admin` / `Technician` / `User`.

### 2.2. Quên / đặt lại mật khẩu

| Mới | Cũ |
|---|---|
| `/forgot-password`, `/reset-password` | `quenmatkhau.aspx` |
| API: `POST /api/auth/forgot-password`, `reset-password` | Form gửi email / đặt lại trên WebForms |

### 2.3. Đổi mật khẩu / hồ sơ

| Mới | Cũ |
|---|---|
| `/profile` | `doimatkhau.aspx` |
| API đổi mật khẩu user hiện tại | Form đổi mật khẩu trong session |

### 2.4. Quản lý tài khoản (Admin)

| Mới | Cũ |
|---|---|
| Menu **Tài khoản** → `/users` | `taikhoan.aspx` |
| API `/api/users` | CRUD `Taikhoan` |

**Giải thích:** Cũ mật khẩu có thể lưu plain text; mới dùng **BCrypt** khi tạo/đổi (tương thích verify khi login).

---

## 3. Tổng quan (Dashboard)

| Mới | Cũ |
|---|---|
| Menu **Tổng quan** → `/dashboard` | `Dashboard.aspx` |
| API `/api/dashboard/summary` | Query/thống kê trên Dashboard |

**Giải thích chi tiết:**

- Hiển thị tổng số thiết bị, sắp hết hạn kiểm định.
- Biểu đồ **Theo trạng thái** (pie) ≈ thống kê theo `DMTrangthai`.
- Biểu đồ **Theo loại thiết bị** (bar) ≈ thống kê theo `DMLoaiTB`.
- Bảng/biểu đồ **Theo đơn vị** ≈ thống kê theo khoa/đơn vị sử dụng.
- Cảnh báo hết hạn kiểm định trên dashboard cũ từng gắn logic gửi mail lúc login; **mới chuyển sang Background Job** (`DeviceExpiryNotificationJob`) chạy định kỳ, không còn phụ thuộc trang login.

---

## 4. Thiết bị (module trung tâm)

Đây là module lớn nhất. Trên hệ mới, hầu hết thao tác quanh thiết bị tập trung ở:

- Danh sách: `/devices`
- Chi tiết: `/devices/:id`
- Thêm/sửa: `/devices/new`, `/devices/:id/edit`

### 4.1. Tìm kiếm / danh sách thiết bị

| Mới | Cũ |
|---|---|
| Menu **Thiết bị** → `/devices` (`DeviceListPage`) | `Timkiem.aspx` |
| API `GET /api/devices` | Grid tìm kiếm DevExpress |
| Click tên/mã → `/devices/:id` | Mở `view.aspx?code=...` |

**Giải thích:** Lọc theo từ khóa, đơn vị, loại, trạng thái, hạn kiểm định… Phân trang phía API. User thường chỉ thấy thiết bị thuộc đơn vị được gán (logic ở Service, giống lọc `MaDV` cũ).

### 4.2. Thêm thiết bị

| Mới | Cũ |
|---|---|
| `/devices/new` (`DeviceFormPage`) | `Themdulieu.aspx` |
| API `POST /api/devices` | Insert `Quanlythietbi` |

**Giải thích:** Form nhập thông tin TB (mã, tên, loại, trạng thái, đơn vị, giá, hạn kiểm định, bảo hành, gói thầu…). Sau khi lưu có thể upload **hợp đồng bảo trì** (thư mục `HDBaotri/{id}/`, lưu tên file vào `IDHoso` — tương thích legacy).

### 4.3. Sửa thiết bị

| Mới | Cũ |
|---|---|
| `/devices/:id/edit` | `edit.aspx` |
| API `PUT /api/devices/:id` | Update `Quanlythietbi` |

### 4.4. Xem chi tiết thiết bị

| Mới | Cũ |
|---|---|
| `/devices/:id` (`DeviceDetailPage`) | `view.aspx` |
| API `GET /api/devices/:id` (+ các sub-API) | Nhiều panel trên `view.aspx` |

**Trên trang chi tiết mới còn gộp các chức năng cũ sau:**

| Chức năng trên chi tiết TB (mới) | Trang / luồng cũ | API / hành vi mới |
|---|---|---|
| Gửi yêu cầu / báo sự cố (modal) | `guiyc.aspx` | `POST /api/devices/:id/events` + upload ảnh `Hinh_anh_su_co` |
| Xử lý sự cố (thường từ menu Sự cố) | `xuly.aspx` | `PUT /api/events/:id` + upload `BAO_CAO_SU_CO` |
| Chuyển giao thiết bị (modal) | `chuyengiao.aspx` | `POST /api/devices/:id/transfer` |
| Lịch sử / nhật ký thiết bị | `history.aspx`, `lichsu.aspx` | `GET /api/devices/:id/history` (hoặc logs tương đương) |
| File / tài liệu đính kèm | Upload trên view/edit cũ | `POST /api/devices/:id/files` → `FILELUU/...` + `Quanlyfile` |
| Chụp ảnh thiết bị | `Camera.aspx`, `CaptureNhatky.aspx` (một phần) | Component camera trên chi tiết → Captures |
| QR code thiết bị | Gắn mã trên view / `testqr.aspx` | Hiển thị/cập nhật QR; scan ở `/scan-qr` |
| Hồ sơ pháp lý gắn TB | `hoso.aspx` / `thongtinhs.aspx` | Tab/section legal trên chi tiết + `/legal-documents` |
| Vật tư / phụ kiện TB | `vattu.aspx` | Supplies trên chi tiết (`/api/devices/:id/supplies`) |
| Nhật ký sử dụng theo TB | `SudungTB.aspx` / panel liên quan | Panel `DeviceUsageLogsPanel` trên chi tiết |
| In phiếu sửa chữa / kiểm định | In từ view/báo cáo cũ | `GET /api/reports/devices/:id/print/...` |
| Import Excel hàng loạt | `import.aspx`, `importdata.aspx` | `POST /api/devices/import` (từ UI thiết bị/báo cáo tùy phiên bản) |

### 4.5. Import thiết bị

| Mới | Cũ |
|---|---|
| Import qua API/UI thiết bị | `import.aspx`, `importdata.aspx` |
| API `POST /api/devices/import` | Parse Excel → insert nhiều dòng |

---

## 5. Sự cố / yêu cầu (Phần 8 và liên quan)

| Mới | Cũ |
|---|---|
| Menu **Sự cố / yêu cầu** → `/events` | `log.aspx` (danh sách), `loga.aspx` (biến thể/admin) |
| Nút **Xử lý** trên dòng → modal | `xuly.aspx` |
| Nút **Xuất Excel** | Xuất từ báo cáo / grid cũ (trước đây trên mới chỉ có ở `/reports`) |
| Nút **Từ khóa sự cố** | Quản lý từ khóa (thường gắn form sự cố / master data cũ) |
| Báo sự cố từ chi tiết TB | `guiyc.aspx` |

### 5.1. Danh sách sự cố

- Lọc trạng thái (chờ xử lý / đã xử lý).
- Hiển thị mã TB, tên, loại yêu cầu, nội dung, thời gian.
- API: `GET /api/events`.

### 5.2. Báo sự cố (guiyc)

- Mở từ **Thiết bị → Chi tiết → Gửi yêu cầu / báo sự cố**.
- Chọn loại (báo hỏng, kiểm tra…), từ khóa, nội dung.
- Upload **hình ảnh sự cố** → thư mục kiểu legacy `Hinh_anh_su_co` (trên mới lưu dưới `FILELUU/{deviceId}/Hinh_anh_su_co/` + ghi `Quanlyfile`).
- API: `POST /api/devices/:id/events`, `POST /api/devices/:id/incident-files?kind=image`.

### 5.3. Xử lý sự cố (xuly)

- Mở từ **Sự cố / yêu cầu → Xử lý**.
- Nhập nội dung xử lý, chi phí.
- Upload **báo cáo sự cố** → `BAO_CAO_SU_CO`.
- API: `PUT /api/events/:id`, `POST /api/events/:id/files` (`kind=report`).

### 5.4. Từ khóa sự cố

| Mới | Cũ |
|---|---|
| Modal trên `/events` (thêm nhanh) | Danh mục từ khóa dùng khi nhập sự cố |
| Admin **Danh mục → Từ khóa sự cố** | CRUD đầy đủ hơn |
| API `GET/POST /api/events/keywords`, `/api/master/event-keywords`, `/api/admin/master/event-keywords` | Bảng `EventKeyword` |

### 5.5. Xuất Excel sự cố

| Mới | Cũ |
|---|---|
| Nút trên `/events` **và** tab Báo cáo | Export danh sách log/sự cố |
| API `GET /api/reports/events/export` | Excel từ grid DevExpress / báo cáo |

---

## 6. Nhật ký sử dụng

Có **hai lớp** cần phân biệt (dễ nhầm với hệ cũ):

| Khái niệm | Mới | Cũ | Bảng / ý nghĩa |
|---|---|---|---|
| Nhật ký sử dụng theo ngày (số liệu) | Menu **Nhật ký sử dụng** `/usage-logs` + panel trên chi tiết TB | `NhatkyTB.aspx`, một phần `SudungTB.aspx` | `InputByDate` — nhập số liệu sử dụng theo ngày/mã TB |
| Nhật ký / lịch sử thao tác TB | Panel lịch sử trên chi tiết | `history.aspx`, `lichsu.aspx` | Log chuyển giao / sự kiện |

**Giải thích:** `/usage-logs` cho phép chọn thiết bị, xem và upsert nhật ký sử dụng. Panel trên chi tiết TB phục vụ thao tác nhanh khi đang xem một máy.

> **Cố ý chưa mang sang (hoặc mang một phần):** `CaptureNhatky.aspx` — luồng chụp/ghi nhật ký chuyên biệt trên mobile cũ; camera chụp ảnh TB đã có trên chi tiết, nhưng parity 100% CaptureNhatky có thể chưa đủ.

---

## 7. Báo cáo & kiểm kê

### 7.1. Menu Báo cáo `/reports`

Gộp nhiều trang cũ thành **các tab**:

| Tab / nút trên `/reports` | Trang cũ gần nhất | API chính |
|---|---|---|
| Tổng hợp / xuất DS thiết bị, TS, sắp hết hạn, sự cố | `baocao.aspx`, export grid | `/api/reports/devices/*`, `assets/*`, `events/export` |
| **Số liệu tổng hợp** (`BaocaoStatisticsTab`) | Các thống kê sâu trong `baocao.aspx` / `baocaoTS.aspx` | `/api/reports/baocao/*` |
| Lưới báo cáo | Grid lọc thiết bị kiểu báo cáo | `/api/reports/devices/grid` |
| Kho TVT (export) | `baocaokho.aspx` | `/api/reports/warehouse/export` |

### 7.2. Kiểm kê tài sản `/asset-inventory`

| Mới | Cũ |
|---|---|
| Menu **Kiểm kê tài sản** | Thống kê / xuất kiểm kê trong nhóm báo cáo tài sản (`baocaoTS.aspx` và biến thể) |
| API `/api/asset-inventory` | Lọc theo khoa, nguồn vốn, cơ sở… rồi xuất Excel |

**Giải thích:** Trên mới tách thành menu riêng để thao tác kiểm kê rõ ràng hơn so với việc chìm trong trang báo cáo TS cũ.

---

## 8. Quét QR

| Mới | Cũ |
|---|---|
| Menu **Quét QR** → `/scan-qr` | `scanqr.aspx` (+ `testqr.aspx` thử nghiệm) |
| Frontend: `@zxing/browser` | Webcam / plugin quét trên WebForms |
| Sau khi quét thường điều hướng tới chi tiết TB | Mở `view.aspx` theo mã |

**Giải thích:** Backend chủ yếu lưu/đọc mã QR trong `QuanlyMa`; việc generate/hiển thị QR có thể làm trên frontend (`qrcode.react`).

---

## 9. Gói thầu

| Mới | Cũ |
|---|---|
| Menu **Gói thầu** → `/bidding` | `goithau.aspx` |
| Thêm/sửa trong UI mới | `themmoigoithau.aspx`, `chinhsuagoithau.aspx` |
| API `/api/bidding-packages` (hoặc `/api/bidding`) | CRUD `Goithau`, gắn thiết bị `idGoithau` |

---

## 10. Hồ sơ pháp lý

| Mới | Cũ |
|---|---|
| Menu **Hồ sơ pháp lý** → `/legal-documents` | `hoso.aspx`, `thongtinhs.aspx` |
| Gắn/xem trên chi tiết thiết bị | Liên kết hồ sơ theo thiết bị trên view cũ |
| API legal-document + upload file | Lưu file hồ sơ + bảng `ThongtinHS` / liên kết |

---

## 11. Kho TVT (thuốc / vật tư)

| Mới | Cũ |
|---|---|
| Menu **Kho TVT** → `/warehouse` | `Nhaphanghoa.aspx` (nhập), `Xuathanghoa.aspx` (xuất) |
| Tab/export kho trong Báo cáo | `baocaokho.aspx` |
| API warehouse | Phiếu nhập/xuất + báo cáo theo khoảng ngày |

---

## 12. Ký số PDF

| Mới | Cũ |
|---|---|
| Menu **Ký số PDF** → `/sign-document` | `sign.aspx` |
| Có thể ký từ danh sách file trên chi tiết TB | Ký file đã upload |
| API ký số (iText) | Ký số trên WebForms |

---

## 13. Đơn vị / khoa phòng

| Mới | Cũ |
|---|---|
| Menu **Đơn vị** → `/organizations` (Admin) | `donvi.aspx`, `khoaphong.aspx` |
| API `/api/organizations` | CRUD `QuanlyDV`, loại đơn vị `DanhmucDV` |

**Giải thích:** Cũ thường tách “đơn vị” và “khoa phòng”; mới thống nhất một module tổ chức (lọc theo loại nếu cần).

---

## 14. Nhân sự

| Mới | Cũ |
|---|---|
| Menu **Nhân sự** → `/personnel` | `quanlynhansu.aspx`, `thongtinnhansu.aspx` |
| API `/api/personnel` | CRUD `QuanlyNhansu` |

---

## 15. Danh mục (master data) — Admin

| Tab trên `/admin/master` | Nguồn dữ liệu cũ (bảng) | Ý nghĩa |
|---|---|---|
| Loại thiết bị | `DMLoaiTB` | Phân loại máy |
| Trạng thái TB | `DMTrangthai` | Đang hoạt động / hỏng / chưa dùng… |
| Loại đơn vị | `DanhmucDV` | Phân loại tổ chức |
| Nước sản xuất | `Country` | Xuất xứ |
| Loại nhiệm vụ / tác vụ | `DMTacvu` | Loại tác vụ |
| Từ khóa sự cố | `EventKeyword` | Gợi ý khi báo/xử lý sự cố |
| Loại nhân sự | `DanhmucLoaiNS` | Phân loại NS |
| Loại hồ sơ | `DanhmucHoso` | Phân loại hồ sơ pháp lý |

**Giải thích:** Trên hệ cũ, nhiều danh mục được sửa rải rác hoặc hard-code trong form. Hệ mới gom **một trang Danh mục** (Admin) để CRUD tập trung, đồng thời vẫn dùng các API `GET /api/master/*` cho dropdown toàn hệ thống.

---

## 16. Nhật ký hệ thống

| Mới | Cũ |
|---|---|
| Menu **Nhật ký hệ thống** → `/admin/system-logs` | Bảng `LogHethong` (thường ghi ngầm, ít UI) |
| API `/api/admin/system-audit-logs` (hoặc tương đương) | Ghi khi tạo/sửa/xử lý… |

**Giải thích:** Mới có UI xem audit (ai làm gì, lúc nào, thiết bị liên quan) — thuận tiện vận hành hơn so với tra DB thủ công.

---

## 17. Thông báo

| Mới | Cũ |
|---|---|
| Icon chuông trên Header + `/notifications` | `thongbao.aspx` |
| API `/api/notifications` | Bảng `Thongbao` |
| Push FCM (nếu cấu hình Firebase) | Push qua `RegID` trên `Taikhoan` |

**Giải thích:** Khi có sự cố mới / xử lý xong / chuyển giao, hệ mới có thể gửi notification in-app + push (Firebase Admin v1), tương tự ý đồ hệ cũ.

---

## 18. Các trang cũ khác — trạng thái trên hệ mới

| Trang cũ | Trên hệ mới | Ghi chú |
|---|---|---|
| `Default.aspx` | Redirect `/` → `/dashboard` | Trang vào mặc định |
| `qltbi.aspx` | Thường là shell/menu cũ | Thay bằng `MainLayout` React |
| `viewrp.aspx` | In/xem báo cáo qua API print/export | Không còn trang riêng cùng tên |
| `location.aspx` | Chưa/ít dùng trên menu mới | Kiểm tra nếu bệnh viện còn dùng vị trí đặc thù |
| `upload.aspx` | Upload gắn vào từng module (TB, sự cố, hồ sơ…) | Không còn trang upload chung |
| `test.aspx`, `testqr.aspx`, `lib/ui/*` | Không mang sang | Trang thử nghiệm / thư viện |
| `Captures/FolderTB/logi.aspx` | Captures phục vụ chụp ảnh TB | Đường dẫn tĩnh `/captures` trên API |
| `temp/chuyengiao.aspx` | Bản nháp cũ | Dùng `TransferDeviceModal` |

---

## 19. Map API chính (để tra cứu nhanh)

| Nhóm chức năng | Endpoint gốc mới |
|---|---|
| Auth | `/api/auth/*` |
| Dashboard | `/api/dashboard/*` |
| Thiết bị | `/api/devices/*` |
| Sự cố | `/api/events/*` |
| Báo cáo | `/api/reports/*`, `/api/reports/baocao/*` |
| Kiểm kê | `/api/asset-inventory/*` |
| Nhật ký sử dụng | `/api/usage-logs`, `/api/devices/:id/usage-logs` |
| Đơn vị | `/api/organizations` |
| Nhân sự | `/api/personnel` |
| Tài khoản | `/api/users` |
| Gói thầu | `/api/bidding*` |
| Kho | `/api/warehouse*` |
| Hồ sơ pháp lý | `/api/legal*` / legal-document trên devices |
| Thông báo | `/api/notifications` |
| Danh mục đọc | `/api/master/*` |
| Danh mục Admin CRUD | `/api/admin/master/*` |
| Nhật ký hệ thống | `/api/admin/system-audit-logs` (hoặc tên tương đương trong code) |
| File tĩnh | `/files/*`, `/hdbaotri/*`, `/captures/*` |

---

## 20. Khác biệt quan trọng khi dùng hệ mới

1. **Một trang thay nhiều trang:** Ví dụ chi tiết thiết bị thay cho `view` + một phần `guiyc` / `chuyengiao` / camera / supplies.
2. **Phân quyền rõ theo role trên menu:** User thường không thấy Báo cáo / Gói thầu / Kho / Admin; Admin thấy Đơn vị, Danh mục, Tài khoản, Nhật ký hệ thống.
3. **File sự cố:** Cũ lưu `TAILIEU/{id}/...`; mới lưu dưới `FILELUU/{id}/Hinh_anh_su_co|BAO_CAO_SU_CO/` và đồng bộ metadata vào `Quanlyfile`.
4. **Hợp đồng bảo trì:** Vẫn tương thích thư mục `HDBaotri/{id}/` như legacy.
5. **Email hết hạn kiểm định:** Job nền, không còn “bắn” khi mở login.
6. **Không server-render:** Mọi màn hình là React; cần API chạy (IIS) + đúng `VITE_API_BASE_URL` khi deploy tách cổng.

---

## 21. Cách dùng tài liệu này khi kiểm thử

1. Mở một menu trên hệ **mới**.
2. Tra bảng mục 1 để biết trang `.aspx` cũ tương ứng.
3. Đọc mục chi tiết (3–17) để biết thao tác nằm ở đâu (trang / modal / tab).
4. Nếu thiếu hành vi so với cũ: ghi rõ tên `.aspx` + bước thao tác cũ → đối chiếu API trong mục 19.

---

*Cập nhật theo codebase QLTBYT_NEW (React + QLTBYT.API) và project WebForms gốc QLTBTYT/QLTBYT.*
