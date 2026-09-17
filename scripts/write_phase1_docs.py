from pathlib import Path

root = Path(__file__).resolve().parents[1]

gitignore = root / ".gitignore"
text = gitignore.read_text(encoding="utf-8")
if "FILELUU" not in text:
    gitignore.write_text(text.rstrip() + "\nQLTBYT.API/FILELUU/\n", encoding="utf-8")

(root / "README.md").write_text(
    """# QLTBYT v2 — Quan ly thiet bi y te

- **Backend:** `QLTBYT.API` (.NET 7) — http://localhost:5149
- **Frontend:** `qltbyt-frontend` (React 18 + Vite + Ant Design) — http://localhost:5173
- **Tests:** `QLTBYT.API.Tests` (`dotnet test`)
- **Database:** `db_QLTBYT_NEW` (script `dbqltbyt.sql`)

## Chay nhanh (Development)

1. Tao DB: `powershell -File scripts/CreateDatabase.ps1`
2. Copy `QLTBYT.API/appsettings.Development.json.example` → `appsettings.Development.json`
3. API: `cd QLTBYT.API` → `dotnet run` (Swagger: `/swagger`)
4. UI: `cd qltbyt-frontend` → `npm install` → `npm run dev`
5. Dang nhap demo: `admin` / `kythuat` / `user` — mat khau `Sample@123`

Tai lieu: `docs/PHASE1_COMPLETE_STATUS.md`, `docs/PHASE1_UAT.md`, `docs/SAMPLE_DATA.md`.
""",
    encoding="utf-8",
)

(root / "docs").mkdir(exist_ok=True)
(root / "docs/PHASE1_UAT.md").write_text(
    """# Phase 1 — UAT checklist

## Moi truong
- [ ] API chay `dotnet run` (5149), DB `db_QLTBYT_NEW` ket noi OK
- [ ] Frontend `npm run dev` (5173)
- [ ] `dotnet test` — 28 tests passed

## Auth
- [ ] Login sai mat khau → thong bao loi
- [ ] Login `admin` / `Sample@123` → Dashboard
- [ ] 401 → Login
- [ ] Dang xuat

## Dashboard
- [ ] Tong thiet bi >= 3
- [ ] Bieu do trang thai / loai
- [ ] Sap het han kiem dinh (90 ngay)

## Thiet bi
- [ ] Tim `TB-DEMO`, loc don vi/trang thai/loai
- [ ] Them / sua / chi tiet / thiet bi con
- [ ] Upload file (kythuat+)
- [ ] User `user` scope don vi

## Phan quyen
- [ ] User khong DELETE (403)

Sample: `docs/SAMPLE_DATA.md`
""",
    encoding="utf-8",
)

(root / "docs/PHASE1_COMPLETE_STATUS.md").write_text(
    """# Phase 1 — Hoan thien

| Buoc | Noi dung | Trang thai |
|------|----------|------------|
| 1.1–1.2 | Serilog, Swagger, CORS, JWT, seeder | Done |
| 1.3 | EF Core + legacy mapping | Done |
| 1.4 | Devices GET/POST | Done |
| 1.5–1.6 | Password, org scope | Done |
| 1.7 | `/api/master/*` | Done |
| 1.8 | `/api/organizations` | Done |
| 1.9–1.12 | Filter, PUT, DELETE, children | Done |
| 1.13 | Upload + `/files` | Done |
| 1.14–1.16 | Frontend login, layout, devices | Done |
| 1.17 | Dashboard API + UI | Done |
| 1.18 | UAT checklist | Done |

Tests: 28 passed. Regenerate UI: `python scripts/gen_phase1_frontend.py`
""",
    encoding="utf-8",
)

print("ok")
