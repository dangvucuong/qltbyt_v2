# Phase 3 — Trạng thái

**Trạng thái:** Hoàn thành 3.1–3.13 + 3.2 hồ sơ pháp lý (cốt lõi) + 3.7 báo cáo tài sản Excel. UAT theo checklist.

## Backend
- Đơn vị CRUD, gói thầu, báo cáo (danh sách, sắp hết hạn, **tài sản**)
- Thiết bị: chuyển giao, lịch sử, vật tư, QR, import Excel
- **Hồ sơ pháp lý:** `api/legal-documents`, liên kết TB, upload file
- Danh mục admin (loại / trạng thái)
- Seed: `GT-DEMO-2024`, hồ sơ demo cho `TB-DEMO-001`

## Frontend
- `/organizations`, `/bidding`, `/reports`, `/admin/master`, **`/legal-documents`**
- Chi tiết TB: tab hồ sơ pháp lý + các tab Phase 3 trước

## Tests
- `Phase3IntegrationTests` (+ hồ sơ pháp lý), tổng **33** passed

## Tài liệu
- `docs/HUONG_DAN_TEST_PHASE3.md`
- `docs/PHASE3_DEFERRED.md` (3.14 UserOrganization)

## Chưa làm (cố ý)
- 3.14 refactor `UserOrganization` — xem `PHASE3_DEFERRED.md`
