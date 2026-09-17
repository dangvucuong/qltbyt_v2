# Phase 1.1–1.2 (Swagger, Serilog, JWT) — Done

## Delivered
- Swagger UI (Development): `/swagger`, JWT Bearer scheme
- Serilog: console + `logs/qltbyt-.log`
- JWT auth: `POST /api/auth/login`, `GET /api/auth/me`
- Policies: AdminOnly, TechnicianAndAbove, AllUsers
- Sample data: `DevelopmentDataSeeder` (Development only) — see `SAMPLE_DATA.md`
- DB patch: `scripts/patch_matkhau_bcrypt.sql` (Matkhau NVARCHAR(200) for BCrypt)

## Tests
`dotnet test QLTBYT.sln` — 8 passed (Step 0 + Auth integration)

## Manual check
1. `dotnet run --project QLTBYT.API`
2. Open https://localhost:7xxx/swagger
3. Login with admin / Sample@123, Authorize with token, call `/api/auth/me`