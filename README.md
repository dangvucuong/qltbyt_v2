# QLTBYT_NEW — Quản lý thiết bị y tế

- **Backend:** QLTBYT.API (.NET 7) — http://localhost:5149
- **Frontend:** qltbyt-frontend (React 18 + Vite + Ant Design) — http://localhost:5173
- **Tests:** QLTBYT.API.Tests (dotnet test)
- **Database:** db_QLTBYT_NEW (script dbqltbyt.sql)

## Chạy nhanh (Development)

1. Cấu hình DB trong QLTBYT.API/appsettings.Development.json
2. API: cd QLTBYT.API → dotnet run
3. UI: cd qltbyt-frontend → 
pm install → 
pm run dev
4. Đăng nhập demo: dmin / kythuat / user — mật khẩu Sample@123

## Test & nghiệm thu (trước Phase 3)

**Tài liệu chính:** [docs/HUONG_DAN_TEST_PHASE1_PHASE2.md](docs/HUONG_DAN_TEST_PHASE1_PHASE2.md)

- Checklist rút gọn: [docs/PHASE1_UAT.md](docs/PHASE1_UAT.md), [docs/PHASE2_UAT.md](docs/PHASE2_UAT.md)
- Dữ liệu mẫu: [docs/SAMPLE_DATA.md](docs/SAMPLE_DATA.md)
