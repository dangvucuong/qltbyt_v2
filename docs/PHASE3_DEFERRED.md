# Phase 3 — Hạng mục tùy chọn / chưa refactor

## 3.14 UserOrganization (MaDV chuỗi)

- **Hiện tại:** Tài khoản `user` dùng `MaDV` / `organizationIds` kiểu chuỗi, lọc TB qua `ICurrentUserService`.
- **Chưa làm:** Bảng `UserOrganization` và refactor phân quyền đa đơn vị.
- **Khi cần:** Migration + cập nhật `UserAccountService` và form tài khoản.

## Mở rộng hồ sơ pháp lý (legacy)

Bảng `ThongtinHS` còn nhiều cột legacy chưa map entity. Phase 3 có CRUD cốt lõi + file; bổ sung khi cần parity `hoso.aspx`.