Tài liệu chuyển đổi QLTBYT — Từng bước cụ thể
Phiên bản: 1.0 · 03/08/2026
Nguồn: D:\DVC\PROJECT\QLTBTYT\QLTBYT\QLTBYT (~52 .aspx, ~116 WebMethod trong App_Code/service.vb)
Đích: D:\DVC\PROJECT\QLTBYT_NEW
DB: db_QLTBYT từ dbqltbyt.sql (giữ schema)
Chuẩn: .cursorrules v2.0

1. Tổng quan
Kiến trúc: React 18 (Ant Design) ↔ REST + JWT ↔ .NET 7 Web API (EF Core) ↔ SQL Server.

Nguyên tắc: Hệ cũ có thể chạy song song cùng DB/file; nghiệp vụ lấy từ .aspx.vb + service.vb (không copy SQL nối chuỗi); lọc theo MaDV trong Service; toàn bộ UI tiếng Việt.

2. Bước 0 — Chuẩn bị (8 bước)
Mục tiêu phase: Chuẩn bị môi trường và bản đồ nghiệp vụ — chưa làm chức năng cho người dùng.

Bước	Chức năng (bước này làm gì)	Việc cụ thể	Kết quả / nghiệm thu
0.1
Database sẵn sàng
Chạy/restore dbqltbyt.sql → db_QLTBYT; kiểm tra bảng Quanlythietbi, Taikhoan, QuanlyDV, LogThietbi, view v_report, v_Logthietbi
Kết nối từ máy dev; SELECT bảng core OK
0.2
Inventory trang cũ
Liệt kê ~52 .aspx; mô tả nghiệp vụ; gán Phase; route React; đánh dấu bỏ (test.aspx, testqr, temp/, lib/ui)
Mỗi trang production có Phase + route
0.3
Inventory API cũ
Liệt kê ~116 WebMethod trong service.vb; nhóm Auth, TB, Đơn vị, NS, TK, Log, Vật tư, Kho, Email/Push, QR, File
File checklist map sang REST
0.4
Chốt nghiệp vụ
Ghi rõ: DMTrangthai; LogThietbi.Loai & Trangthai; LoaiTK → Admin/Technician/User; MaKT (email KĐ); MaDV nhiều ID cách ;
BUSINESS_RULES.md — dev không hỏi lại enum cơ bản
0.5
Bảo mật & config
Template appsettings (placeholder); .env VITE_API_BASE_URL; .gitignore secret, Firebase key
Không commit password DB/SMTP
0.6
Chiến lược file
Quyết path: TAILIEU/{IDTB}/Hinh_anh_su_co/, FILELUU/, …; FileStorage.BasePath có thể trỏ site cũ
Ghi DEPLOYMENT.md
0.7
Migrate mật khẩu
Taikhoan.Matkhau plain text → BCrypt (script hoặc bridge lần login đầu)
User cũ login API mới
0.8
Repo & README
Git QLTBYT_NEW; README: sau Phase 1 chạy API + npm run dev (5173), Swagger
Onboarding dev
3. Phase 1 — Core (18 bước, ~4–6 tuần)
Mục tiêu phase: Đăng nhập JWT, quản lý thiết bị (danh sách, thêm, sửa, xem, lọc), upload ảnh/file cơ bản, dashboard tổng quan.

Bước	Chức năng	Map hệ cũ	Đầu ra
1.1
Tạo project backend .NET 7 QLTBYT.API, folder theo .cursorrules
—
dotnet build
1.2
Pipeline: Serilog, ErrorHandlingMiddleware, CORS localhost:5173, Swagger
—
Swagger + JSON lỗi thống nhất
1.3
EF Core: AppDbContext, entity Domain (Device, User, Org, DM, DeviceFile, DeviceCode…)
LayTTThietbi, GetFieldNameTB…
Query EF không lỗi map legacy
1.4
ApiResponse<T>, PagedResult<T>, DTO Device, AutoMapper, FluentValidation
—
POST lỗi → Errors; list có TotalCount
1.5
Đăng nhập JWT: login, đổi/quên MK; claims role từ LoaiTK
Dangnhap, login.aspx (không gắn email KĐ ở login)
Token + [Authorize]
1.6
Policies + lọc TB theo MaDV (User chỉ đơn vị được gán)
Timkiem + session/LoaiTK
User role 3 không thấy khoa khác
1.7
GET master data (loại TB, trạng thái, country, loại DV)
Laytenloaitb, Laytttrangthai…
Form có dropdown
1.8
GET danh sách đơn vị (đọc)
Laytendonvi, donvi (đọc)
Filter “đơn vị sử dụng”
1.9
Danh sách + tìm kiếm TB: page, search, filter, sort, sắp hết hạn KĐ
Timkiem.aspx, qltbi.aspx
GET /api/devices
1.10
Thêm thiết bị
Themdulieu.aspx, Themthietbi
POST /api/devices
1.11
Chi tiết + sửa + xóa (admin/tech đầy đủ; user subset như Capnhatthietbi_user)
view.aspx, edit.aspx
GET/PUT/DELETE
1.12
Thiết bị con (Thietbichinh)
Tab view/edit
GET /children
1.13
Upload ảnh/file + Quanlyfile, local storage
upload.aspx, Thumb/
POST image, GET files
1.14
Tạo qltbyt-frontend: Vite, React 18, TS strict, AntD, Router, Query, Zustand, Axios
—
npm run dev
1.15
Layout, sidebar, route guard, axiosClient JWT, 401 → login
main.master, login.aspx
Chưa login không vào /devices
1.16
UI: DeviceList, Create, Edit, Detail (tiếng Việt)
4 trang TB cũ
E2E CRUD trên UI
1.17
Dashboard summary + chart (Recharts)
Dashboard.aspx
/api/dashboard/*, route /dashboard
1.18
UAT Phase 1 (Admin / Technician / User)
—
Checklist UAT ghi kết quả
4. Phase 2 — Vận hành (14 bước, ~3–4 tuần)
Mục tiêu phase: Sự cố/yêu cầu, nhật ký sử dụng, thông báo, email/push, tài khoản, nhân sự, audit.

Bước	Chức năng	Map cũ	Endpoint / UI chính
2.1
Gửi yêu cầu/sự cố (loại, tác vụ, keyword, ảnh)
guiyc.aspx, LogThietbi
POST devices/{id}/events, /devices/:id/report
2.2
Xử lý sự cố (trạng thái, nội dung, chi phí, file)
xuly.aspx, Xulythietbi
PUT /api/events/{id}, resolve page
2.3
Danh sách sự cố toàn hệ thống
log.aspx, loga.aspx
GET /api/events, /events
2.4
Quản lý event keywords
EventKeyword
GET/POST events/keywords
2.5
Nhật ký sử dụng theo ngày (giờ/ca)
NhatkyTB.aspx, InputByDate
usage-logs
2.6
Nhật ký text/ảnh (nếu còn dùng)
InsertNhatkyTB, CaptureNhatky
Parity hoặc ghi deprecated
2.7
Push Firebase Admin (sự cố mới/xử lý)
USendNoitificationIOS, RegID
Service push
2.8
Email MailKit (flow sự cố)
SendMail
SMTP test
2.9
Job email sắp hết hạn kiểm định (6:00, MaKT, 30 ngày)
logic trong login cũ — không trên login mới
DeviceExpiryNotificationJob
2.10
Thông báo + đọc/chưa đọc
thongbao.aspx
/api/notifications, NotificationBell
2.11
CRUD tài khoản + profile/me, đổi MK
taikhoan.aspx, doimatkhau.aspx
/api/users, /users, /profile
2.12
CRUD nhân sự
quanlynhansu.aspx
/api/personnel
2.13
Ghi log hệ thống
InsertLoghethong
Audit thao tác quan trọng
2.14
UAT Phase 2
—
Sự cố + user/NS
5. Phase 3 — Nâng cao (15 bước, ~3–4 tuần)
Mục tiêu phase: Báo cáo Excel, hồ sơ pháp lý, gói thầu, import, QR, chuyển giao, settings danh mục, phân quyền UI.

Bước	Chức năng	Map cũ
3.1
CRUD đơn vị đầy đủ
donvi, khoaphong, Themdonvi…
3.2
Hồ sơ pháp lý ThongtinHS + file
hoso, thongtinhs
3.3
Gói thầu + TB trong gói
goithau, themmoigoithau, chinhsuagoithau
3.4
Vật tư đi kèm TB
vattu.aspx, Quanlyvattu
3.5
Chuyển giao TB + log loại 4
chuyengiao.aspx, Chuyengiaothietbi
3.6
Báo cáo TB + export Excel
baocao.aspx
3.7
Báo cáo tài sản
baocaoTS.aspx
3.8
Báo cáo bảo trì / sắp hết hạn
MocTGBH, hankiemdinh, expiring-soon
3.9
Import Excel TB hàng loạt
import.aspx, importdata.aspx
3.10
QR/NFC (BE lưu, FE hiển thị/scan)
scanqr, QuanlyMa, GetCode
3.11
CRUD master data (admin)
Settings
3.12
Tab lịch sử TB
history, lichsu, devices/{id}/logs
3.13
Phân quyền UI (menu/nút theo role)
—
3.14
(Tùy chọn) Bảng UserOrganization thay MaDV chuỗi
Refactor DB
3.15
UAT Phase 3
Báo cáo, import, QR, chuyển giao
6. Phase 4 — Premium (8 bước, ~2–3 tuần)
Mục tiêu phase: Ký PDF, kho TVT/thuốc, camera, in báo cáo, mobile, production.

Bước	Chức năng	Map cũ
4.1
Ký số PDF (iText7, react-pdf)
sign.aspx
4.2
Kho nhập/xuất/lô TVT
Nhaphanghoa, Xuathanghoa, NhapkhoTVT, Themsolo, baocaokho
4.3
Camera chụp ảnh
Camera.aspx, SaveCapturedImage
4.4
In báo cáo (thay .repx)
viewrp.aspx, Reports/
4.5
Responsive / mobile
—
4.6
Tối ưu (index SQL, cache Query)
—
4.7
Deploy IIS, HTTPS, backup DB + file
—
4.8
Go-live, đào tạo, tắt WebForms khi UAT OK
—
7. Ma trận .aspx → React (tham chiếu)
Chi tiết đầy đủ: .cursorrules mục 11. Rút gọn:

Cũ	Route React	Phase
login.aspx
/login
1
Dashboard.aspx
/dashboard
1
Timkiem.aspx
/devices
1
Themdulieu.aspx
/devices/create
1
edit.aspx
/devices/:id/edit
1
view.aspx
/devices/:id
1
guiyc.aspx
/devices/:id/report
2
xuly.aspx
/events/:id/resolve
2
log.aspx / loga.aspx
/events
2
taikhoan.aspx
/users
2
quanlynhansu.aspx
/personnel
2
baocao.aspx
/reports/devices
3
import.aspx
/devices/import
3
sign.aspx
/sign-document
4
Endpoint REST đầy đủ: .cursorrules mục 5.

8. MVP & checklist go-live
MVP sớm: Bước 0 + Phase 1 + 2.1–2.3 (sự cố cơ bản).
Go-live đầy đủ: thêm Phase 2 → 3 → 4 theo nhu cầu (kho, ký PDF).
9. Quy trình lặp cho mỗi chức năng nhỏ
Đọc *.aspx.vb + WebMethod trong service.vb.
Ghi input/output, rule, side-effect (email, push, LogHethong).
DTO + FluentValidation.
Service (EF, auth, MaDV).
Controller mỏng + Swagger.
Frontend: types → api → hook → page.
Test 3 role + dữ liệu legacy.
Đánh dấu done trên checklist phase.
Tổng số bước có mã: 8 (Bước 0) + 18 + 14 + 15 + 8 = 57 bước có mô tả chức năng riêng.

Nếu bạn muốn, tôi có thể lưu file này vào repo (sau khi xóa file lỗi trong docs/) hoặc tách thêm INVENTORY_WEBMETHOD.md từ service.vb.