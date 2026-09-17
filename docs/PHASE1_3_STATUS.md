# Phase 1.3 — EF Core domain mapping — Done

## Entities (legacy table names)
| Entity | Table |
|--------|-------|
| Device | Quanlythietbi (expanded columns) |
| DeviceFile | Quanlyfile |
| DeviceCode | QuanlyMa (QR/NFC) |
| Organization | QuanlyDV |
| OrganizationType | DanhmucDV |
| DeviceType | DMLoaiTB |
| DeviceStatus | DMTrangthai |
| Country | Country |
| EventKeyword | EventKeyword |
| TaskType | DMTacvu |
| UserAccount | Taikhoan |

## AppDbContext
- DbSets for all above
- Fluent relationships (Device ↔ Org/Type/Status, Files, Codes)

## Sample data (Development)
Seeder + `DemoSampleDataEnsurer` in tests: master DM, file/QR rows for TB-DEMO-*

## Tests
`EfCoreLegacyMappingTests` (4) — Include queries, demo attachments, master data

## Next
Phase 1.4 — ApiResponse/PagedResult DTOs, AutoMapper, FluentValidation