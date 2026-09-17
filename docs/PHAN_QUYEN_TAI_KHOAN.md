# Phan quyen tai khoan (Taikhoan)

Chi tiet day du: mapping LoaiTK + Admin -> role, menu, API policy.

## Cot legacy

- LoaiTK: loai tai khoan (Web cu: LoaiTK < 2 ~ admin; LoaiTK < 3 ~ ky thuat+)
- Admin: 1 = quan tri toan he thong
- MaDV: ID don vi (nhieu ID, phan cach , hoac ;)

## Role (RoleMapper.ResolveRole)

1. Admin = 1 -> Admin
2. LoaiTK < 2 -> Admin
3. LoaiTK = 2 -> Technician
4. Con lai -> User

Vi du: admin (LoaiTK=0, Admin=1) -> Admin; h3 (2,0) -> Technician; h4 (3,0) -> User.

## Menu (MainLayout)

- Tat ca: Tong quan, Su co, Thiet bi, Quet QR, Nhat ky
- Admin + Technician: Bao cao, Goi thau, Ho so phap ly, Kho TVT, Ky so PDF, Nhan su
- Chi Admin: Don vi, Danh muc, Tai khoan

## Pham vi MaDV

- Admin, Technician: khong loc thiet bi
- User: loc theo MaDV

## Sau khi sua code

Dang xuat va dang nhap lai de JWT cap nhat role.

