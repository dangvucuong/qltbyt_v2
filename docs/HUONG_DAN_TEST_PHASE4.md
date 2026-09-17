# Hướng dẫn test Phase 4 — Premium

**Phiên bản:** 1.0  
**Phạm vi:** 4.1–4.8 (ky PDF, kho TVT, camera, in bao cao, responsive, cache, deploy, go-live).

---

## 1. Tieu chi nghiem thu

| Hang muc | Yeu cau |
|----------|---------|
| Tu dong | `dotnet test` — **35** test passed |
| API | `api/warehouse/*`, `POST /api/devices/{id}/captures`, `POST /api/devices/files/{id}/sign-pdf` |
| UI | `/warehouse`, `/sign-document`, tab Camera & Ky so tren chi tiet TB, In bao cao |
| Demo | Hang `VT-DEMO-001`, lo `LO-DEMO-2024`, ton 500 |

---

## 2. Chuan bi

Giong Phase 3 (`docs/HUONG_DAN_TEST_PHASE3.md` §2).

- API: http://localhost:5149
- FE: http://localhost:5173
- Tai khoan: `admin` / `kythuat` / `user` — `Sample@123`

---

## 3. Checklist

### 3.1 Kho TVT (`/warehouse`)

1. Dang nhap `kythuat`.
2. Menu **Kho TVT** — thong ke ton, bang hang hoa co `VT-DEMO-001`.
3. **Nhap kho** / **Xuat kho** (xuat <= ton).
4. `user` khong thay menu.

### 3.2 Ky so PDF

1. Chi tiet thiet bi demo — tab **Tai lieu**: upload file `.pdf`.
2. Nut **Ky so PDF** hoac trang **Ky so PDF** (nhap ID file tu DB/API).
3. File path cap nhat co `_kyso.pdf`.

### 3.3 Camera

1. Tab **Camera** — Bat camera, Chup va luu.
2. Anh hien trong tab **Tai lieu** (duong dan `Captures/...`).

### 3.4 In bao cao

1. **Bao cao** — **In bao cao** — preview in (sidebar/header an khi print).

### 3.5 Ky thuat van hanh

- `docs/DEPLOY_IIS.md` — deploy thu
- `docs/sql/phase4_indexes.sql` — index (tuy chon)
- `PHASE4_UAT.md` — ky nghiem thu go-live

---

## 4. Lenh test

```powershell
cd QLTBYT.API.Tests
dotnet test
```

Frontend:

```powershell
cd qltbyt-frontend
npm run build
```