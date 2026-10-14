# Hướng dẫn sử dụng — Hệ thống Quản lý Thiết bị Y tế (QLTBYT)

**Đối tượng:** Nhân viên bệnh viện, kỹ thuật viên, ban lãnh đạo  
**Ứng dụng:** Giao diện web React (SPA)

---

## 1. Đăng nhập & tài khoản

1. Mở địa chỉ hệ thống (do IT cung cấp), vào trang **Đăng nhập**.
2. Nhập tên đăng nhập / mật khẩu → vào **Tổng quan**.
3. **Quên mật khẩu:** dùng link trên trang login (nếu đã cấu hình email).
4. **Đổi mật khẩu / hồ sơ:** nút **Tài khoản** trên header → `/profile`.
5. **Đăng xuất:** nút Đăng xuất trên header.

Sau khi IT đổi phân quyền tài khoản, cần **đăng xuất rồi đăng nhập lại** để áp dụng quyền mới.

### Loại tài khoản (LoaiTK)

| Loại | Tên | Được dùng chủ yếu |
|------|-----|-------------------|
| 0 | Ban lãnh đạo | Toàn bộ chức năng quản trị |
| 1 | Tổ kho, thiết bị | Thiết bị, sự cố, gói thầu, báo cáo, nhân sự… |
| 2 | Kỹ sư khu vực | Thiết bị (theo đơn vị), sự cố, báo cáo, kiểm kê |
| 3 | Nhân viên mạng lưới | Thiết bị (theo đơn vị), sự cố, kiểm kê, kho TVT |

---

## 2. Menu theo chức năng

| Menu | Việc làm chính |
|------|----------------|
| **Tổng quan** | Xem số lượng TB, sắp hết hạn kiểm định, biểu đồ trạng thái / loại / đơn vị |
| **Sự cố / yêu cầu** | Theo dõi báo hỏng, xử lý, xuất Excel, quản lý từ khóa |
| **Thiết bị** | Tìm kiếm, thêm/sửa, chi tiết, báo sự cố, chuyển giao, in phiếu |
| **Quét QR** | Quét mã bằng camera → mở chi tiết thiết bị |
| **Kiểm kê tài sản** | Lọc theo khoa / nguồn vốn, xuất Excel kiểm kê |
| **Nhật ký sử dụng** | Nhập/xem số liệu sử dụng theo ngày |
| **Báo cáo / số liệu** | Thống kê, lưới báo cáo, xuất Excel (TB, tài sản, sự cố, kho…) |
| **Gói thầu** | Quản lý gói thầu và thiết bị thuộc gói |
| **Hồ sơ pháp lý** | Giấy kiểm định, nhập khẩu, gắn thiết bị, file đính kèm |
| **Kho TVT** | Nhập / xuất vật tư–thuốc, theo dõi tồn |
| **Ký số PDF** | Ký số file PDF |
| **Nhân sự** | Quản lý nhân sự |
| **Đơn vị** | Khoa / phòng / đơn vị (Admin) |
| **Danh mục** | Loại TB, trạng thái, từ khóa sự cố… (Admin) |
| **Nhật ký hệ thống** | Xem lịch sử thao tác (Admin) |
| **Tài khoản** | Tạo/sửa user (Admin) |
| **Thông báo** (chuông) | Đọc thông báo sự cố / hệ thống |

---

## 3. Thiết bị

### 3.1. Danh sách & tìm kiếm

- Vào **Thiết bị** → lọc theo từ khóa, đơn vị, loại, trạng thái, hạn kiểm định.
- Bấm mã/tên thiết bị để mở **chi tiết**.

### 3.2. Thêm / sửa

- **Thêm:** `/devices/new` (tùy quyền).
- **Sửa:** từ chi tiết → **Chỉnh sửa**.
- Có thể tải **hợp đồng bảo trì** sau khi lưu thiết bị.

### 3.3. Chi tiết thiết bị — các thao tác thường dùng

| Thao tác | Cách làm |
|----------|----------|
| Báo sự cố | Nút **Báo sự cố** → chọn loại, từ khóa, nội dung; có thể upload ảnh |
| Chuyển giao | Nút **Chuyển giao** (kỹ thuật+) |
| In phiếu | Dropdown **In phiếu**: tình trạng (PSC), bàn giao (BCKT), xử lý/nghiệm thu (PNT) |
| Tài liệu | Tab tài liệu → upload / mở file |
| Nhật ký sử dụng | Panel trên chi tiết hoặc menu **Nhật ký sử dụng** |
| Hồ sơ pháp lý | Tab / liên kết tới hồ sơ gắn thiết bị |
| Chụp ảnh | Tab camera (nếu có quyền) |

### 3.4. Import Excel

- Dùng chức năng import trên danh sách thiết bị (kỹ thuật / lãnh đạo) để nhập hàng loạt.

---

## 4. Sự cố / yêu cầu

1. **Báo sự cố:** từ chi tiết thiết bị → **Báo sự cố**.
2. **Danh sách:** menu **Sự cố / yêu cầu** → lọc chờ xử lý / đã xử lý.
3. **Xử lý:** bấm **Xử lý** → nhập nội dung, chi phí; upload file báo cáo nếu cần.
4. **Xuất Excel:** nút **Xuất Excel** trên trang sự cố.
5. **Từ khóa:** nút **Từ khóa sự cố** (kỹ thuật+) — thêm nhanh; Admin sửa/xóa đầy đủ ở **Danh mục**.

---

## 5. Kiểm kê & báo cáo

- **Kiểm kê tài sản:** lọc khoa, nguồn vốn, cơ sở → xem lưới → xuất Excel.  
  *(Nhân viên mạng lưới vẫn vào được kiểm kê như hệ cũ.)*
- **Báo cáo / số liệu:** các tab tổng hợp, số liệu, lưới, xuất Excel thiết bị / tài sản / sắp hết hạn / sự cố / kho.  
  *(Loại tài khoản “Nhân viên mạng lưới” không vào được báo cáo số liệu tổng hợp.)*

---

## 6. Kho TVT

- Quản lý mặt hàng, phiếu nhập, phiếu xuất, tồn kho.
- Quyền theo loại TK: **Ban lãnh đạo** và **Nhân viên mạng lưới** (giống hệ cũ).

---

## 7. Gói thầu, hồ sơ pháp lý, nhân sự

- **Gói thầu:** xem/thêm/sửa gói, xem thiết bị trong gói (Ban lãnh đạo / Tổ kho).
- **Hồ sơ pháp lý:** nhập giấy kiểm định, nhập khẩu…; chọn thiết bị; tải file.
- **Nhân sự:** CRUD thông tin nhân sự (Ban lãnh đạo / Tổ kho).

---

## 8. Quét QR & ký số

- **Quét QR:** cho phép camera → quét mã → mở trang thiết bị tương ứng.
- **Ký số PDF:** chọn file PDF đã upload → ký số theo quy trình trên trang Ký số.

---

## 9. Thông báo

- Icon chuông trên header: xem thông báo mới, đánh dấu đã đọc.
- Thông báo thường xuất hiện khi có sự cố mới hoặc sự cố được xử lý (nếu hệ thống đã bật email/push).

---

## 10. Gợi ý kiểm tra nhanh sau khi nhận bàn giao

| Bước | Việc làm | Kỳ vọng |
|------|----------|---------|
| 1 | Đăng nhập đúng role của mình | Vào được Tổng quan; menu đúng quyền |
| 2 | Mở Thiết bị, tìm một máy quen thuộc | Thấy thông tin đúng |
| 3 | Báo sự cố thử (nếu được phép) | Xuất hiện trong Sự cố |
| 4 | Xử lý sự cố (kỹ thuật) | Trạng thái Đã xử lý |
| 5 | Kiểm kê / Báo cáo (theo quyền) | Lọc và xuất Excel được |
| 6 | In phiếu từ chi tiết TB | Mở trang in, bấm In |

Nếu lỗi đăng nhập hoặc thiếu menu: liên hệ IT, đăng xuất/đăng nhập lại, kiểm tra loại tài khoản (LoaiTK) và đơn vị (`MaDV`).
