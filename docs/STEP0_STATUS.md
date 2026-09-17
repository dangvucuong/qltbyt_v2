# Bước 0 — Trạng thái

## Hoàn thành
- Database `db_QLTBYT_NEW` tạo từ `dbqltbyt.sql` (30 bảng base)
- Script: `scripts/db_QLTBYT_NEW.sql`, `scripts/CreateDatabase.ps1`
- Solution: `QLTBYT.sln`, `QLTBYT.API`, `QLTBYT.API.Tests`
- API: `GET /api/health/database` (kiểm tra DB + bảng core)
- Unit/Integration tests: 5 passed (`dotnet test`)

## Cấu hình
- Connection: `QLTBYT.API/appsettings.Development.json` (gitignored)
- Template: `appsettings.Development.example.json`
- Test đọc connection từ Development.json hoặc `QLTBYT_CONNECTION`

## Tiếp theo: Phase 1 (JWT, CRUD thiết bị)