# Phase 1 — Checklist UAT (rút gọn)

Hướng dẫn **chi tiết**: [HUONG_DAN_TEST_PHASE1_PHASE2.md](./HUONG_DAN_TEST_PHASE1_PHASE2.md)

## Môi trường

- [ ] API `http://localhost:5149`, Swagger OK
- [ ] Frontend `http://localhost:5173`
- [ ] `dotnet test` — 28+ tests passed

## Checklist nhanh

- [ ] Login / logout / 401 → login (`admin` / `Sample@123`)
- [ ] Dashboard: tổng TB, biểu đồ, sắp hết hạn
- [ ] Thiết bị: tìm TB-DEMO, lọc, thêm/sửa/chi tiết/con, upload (kythuat+)
- [ ] User `user`: scope đơn vị, không xóa TB

Dữ liệu mẫu: [SAMPLE_DATA.md](./SAMPLE_DATA.md)
