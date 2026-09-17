# Hướng dẫn test Phase 3 — Nâng cao

**Phiên bản:** 1.1 · QLTBYT_NEW  
**Phạm vi:** 3.1–3.13 (+ hồ sơ pháp lý 3.2, báo cáo tài sản 3.7). 3.14 tùy chọn — `PHASE3_DEFERRED.md`.

---

## 1. Tiêu chí nghiệm thu

| Hạng mục | Yêu cầu |
|----------|---------|
| Tự động | `dotnet test` — **33** test passed |
| API | Đơn vị, gói thầu, hồ sơ pháp lý, báo cáo Excel (DS / sắp hết hạn / tài sản), import, chuyển giao, lịch sử, vật tư, QR |
| UI | Menu theo role; Báo cáo, Gói thầu, Hồ sơ pháp lý, Đơn vị, Danh mục; tab chi tiết TB |
| Demo | Tiếng Việt; `GT-DEMO-2024`; hồ sơ `TB-DEMO-001` (KD-DEMO-2024-001) |

---

## 2. Chuẩn bị

Giống `docs/HUONG_DAN_TEST_PHASE1_PHASE2.md` §2.

- API: `dotnet run` → http://localhost:5149
- FE: `npm run dev` → http://localhost:5173
- Tài khoản: `admin` / `kythuat` / `user` — `Sample@123`

---

## 3. Checklist

### 3.1 Đơn vị (Admin — **Đơn vị**)

CRUD; `user` không thấy menu.

### 3.2 Gói thầu (**Gói thầu**)

`GT-DEMO-2024`, modal TB; Admin thêm `GT-TEST-UAT`.

### 3.2b Hồ sơ pháp lý (**Hồ sơ pháp lý** + tab TB)

| # | Bước | Kỳ vọng |
|---|------|---------|
| 1 | `kythuat` → **Hồ sơ pháp lý** | Danh sách có bản ghi demo |
| 2 | Mở **TB-DEMO-001** → tab **Hồ sơ pháp lý** | Giấy KD/NK/LHH, giá VAT |
| 3 | **Tải file hồ sơ** | Upload OK; link tải file |
| 4 | Sửa / thêm hồ sơ (mã TB phẩy) | Liên kết TB qua `MaThietbi` / `IDHoso` |

### 3.3 Báo cáo (**Báo cáo**)

Excel: danh sách TB; sắp hết hạn 90 ngày; **Báo cáo tài sản thiết bị**.

### 3.4 Import (**Thiết bị** — Import Excel)

`kythuat`/`admin` only.

### 3.5 Chi tiết TB

Chuyển giao, lịch sử, vật tư, QR/NFC, hồ sơ pháp lý.

### 3.6 Danh mục (Admin — **Danh mục**)

Loại TB, trạng thái.

### 3.7 Phân quyền menu

| Role | Menu |
|------|------|
| user | Tổng quan, Sự cố, Thiết bị, Nhật ký |
| kythuat | + Báo cáo, Gói thầu, Hồ sơ pháp lý, Nhân sự, Import, chuyển giao |
| admin | + Đơn vị, Danh mục, Tài khoản |

---

## 4. API

| Endpoint | Ghi chú |
|----------|---------|
| GET/POST/PUT/DELETE `/api/legal-documents` | CRUD hồ sơ |
| POST `/api/legal-documents/{id}/file` | Upload file |
| GET `/api/devices/{id}/legal-document` | Hồ sơ theo TB |
| PUT `/api/devices/{id}/legal-document` | Liên kết `{ legalDocumentId }` |
| GET `/api/reports/assets/export` | Excel tài sản |

---

## 5. Test tự động

```powershell
cd D:\DVC\PROJECT\QLTBYT_NEW
dotnet test QLTBYT.API.Tests\QLTBYT.API.Tests.csproj
```

`Phase3IntegrationTests` (tổ chức, gói thầu, báo cáo, lịch sử, export, **hồ sơ pháp lý**).

---

## 6. Ngoài phạm vi

- **3.14** UserOrganization — `docs/PHASE3_DEFERRED.md`
- Cột `ThongtinHS` legacy chưa map đầy đủ

---

## 7. UAT

| Mục | Pass/Fail |
|-----|-----------|
| Đơn vị | |
| Gói thầu | |
| Hồ sơ pháp lý | |
| Báo cáo (3 file Excel) | |
| Import | |
| Chuyển giao + lịch sử | |
| Vật tư + QR | |
| Danh mục | |
| Menu 3 role | |
| dotnet test 33 | |