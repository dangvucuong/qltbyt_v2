# Deploy IIS (Phase 4.7)

## API (ASP.NET Core 7)

1. Publish: `dotnet publish QLTBYT.API -c Release -o ./publish/api`
2. IIS site: ASP.NET Core Module, pool **No Managed Code**
3. `web.config` do module tao; set `ASPNETCORE_ENVIRONMENT=Production`
4. Connection string trong `appsettings.Production.json` hoac bien moi truong
5. Thu muc ghi: `FILELUU`, `Captures` (cung cap voi publish hoac duong dan rieng)
6. Static: API phuc vu `/files` va `/captures`; FE proxy hoac cung host

## Frontend

1. `npm run build` trong `qltbyt-frontend`
2. Copy `dist/` len IIS site static hoac nginx
3. Cau hinh reverse proxy `/api` -> API backend
4. HTTPS (cert noi bo hoac Let's Encrypt)

## Backup

- SQL Server: backup hang ngay `db_QLTBYT_NEW`
- File: backup `FILELUU` + `Captures`

## Go-live (4.8)

- UAT theo `PHASE4_UAT.md`
- Dao tao user; tat WebForms khi UAT OK