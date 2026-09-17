# -*- coding: utf-8 -*-
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]

p = ROOT / "QLTBYT.API.Tests" / "Infrastructure" / "DemoSampleDataEnsurer.cs"
p.write_text(
    """using QLTBYT.API.Data;
using QLTBYT.API.Infrastructure;

namespace QLTBYT.API.Tests.Infrastructure;

public static class DemoSampleDataEnsurer
{
    public static Task EnsureVietnameseSampleDataAsync(AppDbContext context) =>
        SampleDataVietnameseRefresher.EnsureAllAsync(context);

    public static Task EnsureDemoDeviceAttachmentsAsync(AppDbContext context) =>
        EnsureVietnameseSampleDataAsync(context);

    public static Task EnsureDemoDeviceHierarchyAsync(AppDbContext context) =>
        EnsureVietnameseSampleDataAsync(context);

    public static Task EnsureDeviceStatusLabelsAsync(AppDbContext context) =>
        DeviceStatusLabelEnsurer.EnsureAsync(context);

    public static Task EnsureMasterDataAsync(AppDbContext context) =>
        EnsureVietnameseSampleDataAsync(context);

    public static Task EnsureSampleUserPasswordsAsync(AppDbContext context) =>
        EnsureVietnameseSampleDataAsync(context);
}
""",
    encoding="utf-8",
    newline="\n",
)

sp = ROOT / "QLTBYT.API" / "Infrastructure" / "DevelopmentDataSeeder.cs"
t = sp.read_text(encoding="utf-8")
old = (
    "        var orgIds = await context.Organizations.OrderBy(o => o.Id).Select(o => o.Id).Take(2).ToListAsync(cancellationToken);\n"
    "        var typeId = await context.DeviceTypes.Select(t => t.Id).FirstAsync(cancellationToken);\n"
    "        var statusId = 1;\n\n"
    "        if"
)
new = (
    "        var orgIds = await context.Organizations.OrderBy(o => o.Id).Select(o => o.Id).Take(2).ToListAsync(cancellationToken);\n\n"
    "        if"
)
if old in t:
    t = t.replace(old, new, 1)
    sp.write_text(t, encoding="utf-8", newline="\n")

mp = ROOT / "QLTBYT.API.Tests" / "Phase1" / "MappingProfileTests.cs"
mt = mp.read_text(encoding="utf-8")
for a, b in [
    ("May tho", "M\u00e1y th\u1edf"),
    ("Khoa Noi", "Khoa N\u1ed9i"),
    ("Dang hoat dong", "\u0110ang ho\u1ea1t \u0111\u1ed9ng"),
]:
    mt = mt.replace(a, b)
mp.write_text(mt, encoding="utf-8", newline="\n")

pr = ROOT / "QLTBYT.API.Tests" / "Phase1" / "Phase1RemainingIntegrationTests.cs"
pt = pr.read_text(encoding="utf-8")
pt = pt.replace(
    "Name = \"May tho oxy demo (updated)\"",
    "Name = \"M\u00e1y th\u1edf oxy demo (\u0111\u00e3 c\u1eadp nh\u1eadt)\"",
)
pr.write_text(pt, encoding="utf-8", newline="\n")

(ROOT / "docs" / "SAMPLE_DATA.md").write_text(
    """# Dữ liệu mẫu (Development)

`DevelopmentDataSeeder` + `SampleDataVietnameseRefresher`: chuẩn hóa tiếng Việt có dấu và tạo lại thiết bị `TB-DEMO-*` mỗi lần khởi động Development.

## Tài khoản (`Sample@123`)

| Username | Họ tên |
|----------|--------|
| admin | Quản trị QLTBYT |
| kythuat | Kỹ thuật viên |
| user | Nhân viên khoa |

## Thiết bị demo

| MaTB | Tên |
|------|-----|
| TB-DEMO-001 | Máy thợ oxy demo |
| TB-DEMO-002 | Máy đo huyết áp demo |
| TB-DEMO-003 | Máy siêu âm demo |

## Danh mục

Trạng thái: Đang hoạt động; Không hoạt động / Hỏng; Chưa sử dụng. Loại: Máy thở, Máy đo, Thiết bị hình ảnh. Đơn vị: Khoa Nội, Khoa Cấp cứu, Khoa Hồi sức.

Script: `python scripts/refresh_sample_data_vn.py`
""",
    encoding="utf-8",
    newline="\n",
)

print("patch_tests_docs ok")
