# Phase 1.4 — DTO, PagedResult, AutoMapper, FluentValidation — Done

## Added
- `PagedResult<T>` with `TotalCount`, `TotalPages`
- Device DTOs: list, detail, create/update, query params
- `MappingProfile` (AutoMapper 12)
- Validators: Login, CreateDevice, UpdateDevice
- `InvalidModelStateResponseFactory` → `ApiResponse` + `Errors`
- `DeviceService` + `DevicesController`: GET list/detail, POST create

## API (JWT)
- `GET /api/devices?page&pageSize&search`
- `GET /api/devices/{id}`
- `POST /api/devices` (TechnicianAndAbove)

## Tests
19 passed (PagedResult, Mapping, Validator, Devices integration)

## Note
Paging loads ID list in memory for SQL Server without OFFSET/FETCH support.

## Next
Phase 1.5–1.6 — JWT polish, MaDV filter for User role