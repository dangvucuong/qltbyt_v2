# QLTBYT_NEW — Quản lý thiết bị y tế

- **Backend:** QLTBYT.API (.NET 7)
- **Frontend:** qltbyt-frontend (React 18 + Vite + Ant Design)
- **Tests:** QLTBYT.API.Tests
- **Database:** SQL Server — schema legacy `db_QLTBYT`

## Tài liệu

| Tài liệu | File |
|----------|------|
| Hướng dẫn sử dụng | [docs/HUONG_DAN_SU_DUNG.md](docs/HUONG_DAN_SU_DUNG.md) |
| Tài liệu kỹ thuật | [docs/TAI_LIEU_KY_THUAT.md](docs/TAI_LIEU_KY_THUAT.md) |
| Hướng dẫn deploy | [docs/HUONG_DAN_DEPLOY.md](docs/HUONG_DAN_DEPLOY.md) |

Mục lục nhanh: [docs/README.md](docs/README.md)

## Chạy nhanh (Development)

1. Cấu hình DB trong `QLTBYT.API/appsettings.Development.json` (không commit)
2. API: `cd QLTBYT.API` → `dotnet run` → http://localhost:5149
3. UI: `cd qltbyt-frontend` → `npm install` → `npm run dev` → http://localhost:5173
4. Demo (nếu seed): `admin` / `kythuat` / `user` — mật khẩu `Sample@123`
