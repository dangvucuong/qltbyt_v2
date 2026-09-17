# -*- coding: utf-8 -*-
"""Generate SampleDataVietnameseRefresher and patch DevelopmentDataSeeder. UTF-8 safe."""
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
REFRESHER = ROOT / "QLTBYT.API" / "Infrastructure" / "SampleDataVietnameseRefresher.cs"

REFRESHER_CS = r'''using Microsoft.EntityFrameworkCore;
using QLTBYT.API.Data;
using QLTBYT.API.Helpers;
using QLTBYT.API.Models.Domain;

namespace QLTBYT.API.Infrastructure;

/// <summary>Dong bo danh muc + tai khoan demo + thiet bi TB-DEMO-* (tieng Viet co dau).</summary>
public static class SampleDataVietnameseRefresher
{
    private static readonly Dictionary<string, string> DeviceTypeLegacy = new()
    {
        { "May tho", "\u004d\u00e1y th\u1edf" },
        { "May do", "\u004d\u00e1y \u0111\u1ecd" },
        { "Thiet bi hinh anh", "Thi\u1ebft b\u1ecb h\u00ecnh \u1ea3nh" },
    };

    private static readonly Dictionary<string, string> OrgTypeLegacy = new()
    {
        { "Khoa/Phong", "Khoa/Ph\u00f2ng" },
        { "Trung tam", "Trung t\u00e2m" },
        { "Don vi ngoai", "\u0110\u01a1n v\u1ecb ngo\u00e0i" },
    };

    private static readonly Dictionary<string, string> OrganizationLegacy = new()
    {
        { "Khoa Noi", "Khoa N\u1ed9i" },
        { "Khoa Cap cuu", "Khoa C\u1ea5p c\u1ee9u" },
        { "Khoa Hoi suc", "Khoa H\u1ed3i s\u1ee9c" },
    };

    private static readonly Dictionary<string, string> CountryLegacy = new()
    {
        { "Viet Nam", "Vi\u1ec7t Nam" },
    };

    private static readonly Dictionary<string, string> EventKeywordLegacy = new()
    {
        { "Bao hong", "B\u00e1o h\u1ecfng" },
        { "Kiem tra dinh ky", "Ki\u1ec3m tra \u0111\u1ecbnh k\u1ef3" },
        { "Yeu cau khac", "Y\u00eau c\u1ea7u kh\u00e1c" },
    };

    private static readonly Dictionary<string, string> TaskTypeLegacy = new()
    {
        { "Bao tri", "B\u1ea3o tr\u00ec" },
        { "Sua chua", "S\u1eeda ch\u1eefa" },
        { "Kiem dinh", "Ki\u1ec3m \u0111\u1ecbnh" },
    };

    private static readonly Dictionary<string, string> SampleUserFullNames = new()
    {
        { "admin", "Qu\u1ea3n tr\u1ecb QLTBYT" },
        { "kythuat", "K\u1ef9 thu\u1eadt vi\u00ean" },
        { "user", "Nh\u00e2n vi\u00ean khoa" },
    };

    public static async Task EnsureAllAsync(AppDbContext context, CancellationToken cancellationToken = default)
    {
        await DeviceStatusLabelEnsurer.EnsureAsync(context, cancellationToken);
        await ApplyLegacyMapAsync(context.DeviceTypes, t => t.Name, (t, v) => t.Name = v, DeviceTypeLegacy, cancellationToken);
        await ApplyLegacyMapAsync(context.OrganizationTypes, t => t.Name, (t, v) => t.Name = v, OrgTypeLegacy, cancellationToken);
        await ApplyLegacyMapAsync(context.Organizations, o => o.Name, (o, v) => o.Name = v, OrganizationLegacy, cancellationToken);
        await ApplyLegacyMapAsync(context.Countries, c => c.Name, (c, v) => c.Name = v, CountryLegacy, cancellationToken);
        await ApplyLegacyMapAsync(context.EventKeywords, e => e.Keyword, (e, v) => e.Keyword = v, EventKeywordLegacy, cancellationToken);
        await ApplyLegacyMapAsync(context.TaskTypes, t => t.Name, (t, v) => t.Name = v, TaskTypeLegacy, cancellationToken);
        await EnsureSampleUserDisplayNamesAsync(context, cancellationToken);
        await RecreateDemoDevicesAsync(context, cancellationToken);
        if (context.ChangeTracker.HasChanges())
        {
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    private static async Task ApplyLegacyMapAsync<T>(
        DbSet<T> set,
        Func<T, string?> getName,
        Action<T, string> setName,
        Dictionary<string, string> map,
        CancellationToken cancellationToken)
    {
        var items = await set.ToListAsync(cancellationToken);
        foreach (var item in items)
        {
            var name = getName(item);
            if (name != null && map.TryGetValue(name, out var vn))
            {
                setName(item, vn);
            }
        }
    }

    private static async Task EnsureSampleUserDisplayNamesAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        var users = await context.UserAccounts
            .Where(u => u.Username == "admin" || u.Username == "kythuat" || u.Username == "user")
            .ToListAsync(cancellationToken);

        foreach (var user in users)
        {
            if (SampleUserFullNames.TryGetValue(user.Username, out var fullName))
            {
                user.FullName = fullName;
            }

            if (!PasswordHasher.Verify("Sample@123", user.Password ?? string.Empty))
            {
                user.Password = PasswordHasher.Hash("Sample@123");
                user.RegId = null;
            }
        }
    }

    private static async Task RecreateDemoDevicesAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        var demoDevices = await context.Devices
            .Where(d => d.DeviceCode != null && d.DeviceCode.StartsWith("TB-DEMO-"))
            .ToListAsync(cancellationToken);

        if (demoDevices.Count == 0)
        {
            await SeedDemoDevicesIfMasterReadyAsync(context, cancellationToken);
            demoDevices = await context.Devices
                .Where(d => d.DeviceCode != null && d.DeviceCode.StartsWith("TB-DEMO-"))
                .ToListAsync(cancellationToken);
        }

        foreach (var device in demoDevices)
        {
            var files = await context.DeviceFiles.Where(f => f.DeviceId == device.Id).ToListAsync(cancellationToken);
            var codes = await context.DeviceCodes.Where(c => c.DeviceId == device.Id).ToListAsync(cancellationToken);
            context.DeviceFiles.RemoveRange(files);
            context.DeviceCodes.RemoveRange(codes);
        }

        context.Devices.RemoveRange(demoDevices);
        await context.SaveChangesAsync(cancellationToken);

        await SeedDemoDevicesIfMasterReadyAsync(context, cancellationToken);
        await SeedDemoAttachmentsAsync(context, cancellationToken);
        await EnsureDemoDeviceHierarchyAsync(context, cancellationToken);
    }

    private static async Task SeedDemoDevicesIfMasterReadyAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        if (await context.Devices.AnyAsync(d => d.DeviceCode != null && d.DeviceCode.StartsWith("TB-DEMO-"), cancellationToken))
        {
            return;
        }

        var orgIds = await context.Organizations.OrderBy(o => o.Id).Select(o => o.Id).Take(2).ToListAsync(cancellationToken);
        if (orgIds.Count == 0)
        {
            return;
        }

        var typeIds = await context.DeviceTypes.OrderBy(t => t.Id).Select(t => t.Id).Take(3).ToListAsync(cancellationToken);
        var typeId = typeIds.FirstOrDefault();
        if (typeId == 0)
        {
            return;
        }

        var org1 = orgIds[0];
        var org2 = orgIds.Count > 1 ? orgIds[1] : org1;

        context.Devices.AddRange(
            new Device
            {
                DeviceCode = "TB-DEMO-001",
                Name = "\u004d\u00e1y th\u1edf oxy demo",
                TypeId = typeId,
                OrganizationId = org1,
                StatusId = 1,
                Manufacturer = "Demo Medical",
                CountryOfOrigin = "Vi\u1ec7t Nam",
                ReceivedDate = DateTime.Today.AddMonths(-6),
                InspectionExpiryDate = DateTime.Today.AddMonths(3),
                UpdatedAt = DateTime.UtcNow
            },
            new Device
            {
                DeviceCode = "TB-DEMO-002",
                Name = "\u004d\u00e1y \u0111\u1ecd huy\u1ebft \u00e1p demo",
                TypeId = typeIds.Count > 1 ? typeIds[1] : typeId,
                OrganizationId = org2,
                StatusId = 1,
                CountryOfOrigin = "Vi\u1ec7t Nam",
                ReceivedDate = DateTime.Today.AddMonths(-12),
                InspectionExpiryDate = DateTime.Today.AddDays(20),
                UpdatedAt = DateTime.UtcNow
            },
            new Device
            {
                DeviceCode = "TB-DEMO-003",
                Name = "\u004d\u00e1y si\u00eau \u00e2m demo",
                TypeId = typeIds.Count > 2 ? typeIds[2] : typeId,
                OrganizationId = org1,
                StatusId = 2,
                ParentDeviceCode = "TB-DEMO-001",
                ReceivedDate = DateTime.Today.AddYears(-2),
                InspectionExpiryDate = DateTime.Today.AddMonths(6),
                UpdatedAt = DateTime.UtcNow
            });

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task EnsureDemoDeviceHierarchyAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        var parent = await context.Devices.FirstOrDefaultAsync(d => d.DeviceCode == "TB-DEMO-001", cancellationToken);
        var child = await context.Devices.FirstOrDefaultAsync(d => d.DeviceCode == "TB-DEMO-003", cancellationToken);
        if (parent is null || child is null)
        {
            return;
        }

        child.ParentDeviceCode = parent.DeviceCode;
        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedDemoAttachmentsAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        var demoDevices = await context.Devices
            .Where(d => d.DeviceCode != null && d.DeviceCode.StartsWith("TB-DEMO-"))
            .ToListAsync(cancellationToken);

        foreach (var device in demoDevices)
        {
            context.DeviceFiles.Add(new DeviceFile
            {
                DeviceId = device.Id,
                Path = $"FILELUU/demo/{device.DeviceCode}_huongdan.pdf",
                Description = "T\u00e0i li\u1ec7u h\u01b0\u1edbng d\u1eabn demo",
                FileDate = DateTime.Today
            });

            context.DeviceCodes.Add(new DeviceCode
            {
                DeviceId = device.Id,
                QrCode = $"QR-{device.DeviceCode}",
                Nfc = $"NFC-{device.DeviceCode}"
            });
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
'''

SEEDER_PATH = ROOT / "QLTBYT.API" / "Infrastructure" / "DevelopmentDataSeeder.cs"


def read_text(path: Path) -> str:
    b = path.read_bytes()
    if len(b) > 2 and b[1] == 0:
        return b.decode("utf-16-le")
    return b.decode("utf-8")


def write_text(path: Path, text: str) -> None:
    path.write_text(text, encoding="utf-8", newline="\n")


def patch_seeder(text: str) -> str:
    old_device_block = """        if (!await context.Devices.AnyAsync(cancellationToken))
        {
            var org1 = orgIds.ElementAtOrDefault(0);
            var org2 = orgIds.ElementAtOrDefault(1);
            context.Devices.AddRange(
                new Device
                {
                    DeviceCode = "TB-DEMO-001",
                    Name = "May tho oxy demo",
                    TypeId = typeId,
                    OrganizationId = org1,
                    StatusId = statusId,
                    Manufacturer = "Demo Medical",
                    CountryOfOrigin = "Viet Nam",
                    ReceivedDate = DateTime.Today.AddMonths(-6),
                    InspectionExpiryDate = DateTime.Today.AddMonths(3),
                    UpdatedAt = DateTime.UtcNow
                },
                new Device
                {
                    DeviceCode = "TB-DEMO-002",
                    Name = "May do huyet ap demo",
                    TypeId = typeId,
                    OrganizationId = org2,
                    StatusId = statusId,
                    ReceivedDate = DateTime.Today.AddMonths(-12),
                    InspectionExpiryDate = DateTime.Today.AddDays(20),
                    UpdatedAt = DateTime.UtcNow
                },
                new Device
                {
                    DeviceCode = "TB-DEMO-003",
                    Name = "May sieu am demo",
                    TypeId = typeId,
                    OrganizationId = org1,
                    StatusId = 2,
                    ParentDeviceCode = "TB-DEMO-001",
                    ReceivedDate = DateTime.Today.AddYears(-2),
                    InspectionExpiryDate = DateTime.Today.AddMonths(6),
                    UpdatedAt = DateTime.UtcNow
                });
            await context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Seeded 3 sample devices (TB-DEMO-001..003)");
        }

        await EnsureDemoDeviceHierarchyAsync(context, cancellationToken);

        await SeedDemoAttachmentsAsync(context, cancellationToken);"""

    new_tail = """        await SampleDataVietnameseRefresher.EnsureAllAsync(context, cancellationToken);
        _logger.LogInformation("Sample data refreshed (Vietnamese labels + TB-DEMO devices).");"""

    if old_device_block in text:
        text = text.replace(old_device_block, new_tail, 1)
    elif "SampleDataVietnameseRefresher.EnsureAllAsync" not in text:
        text = text.replace(
            "        await SeedDemoAttachmentsAsync(context, cancellationToken);\n    }",
            new_tail + "\n    }",
            1,
        )

    # Update seed strings to VN escapes where still ASCII
    replacements = [
        ('new DeviceType { Name = "May tho" }', 'new DeviceType { Name = "\\u004d\\u00e1y th\\u1edf" }'),
        ('new DeviceType { Name = "May do" }', 'new DeviceType { Name = "\\u004d\\u00e1y \\u0111\\u1ecd" }'),
        ('new DeviceType { Name = "Thiet bi hinh anh" }', 'new DeviceType { Name = "Thi\\u1ebft b\\u1ecb h\\u00ecnh \\u1ea3nh" }'),
        ('new OrganizationType { Name = "Khoa/Phong" }', 'new OrganizationType { Name = "Khoa/Ph\\u00f2ng" }'),
        ('new OrganizationType { Name = "Trung tam" }', 'new OrganizationType { Name = "Trung t\\u00e2m" }'),
        ('new OrganizationType { Name = "Don vi ngoai" }', 'new OrganizationType { Name = "\\u0110\\u01a1n v\\u1ecb ngo\\u00e0i" }'),
        ('Name = "Viet Nam"', 'Name = "Vi\\u1ec7t Nam"'),
        ('Keyword = "Bao hong"', 'Keyword = "B\\u00e1o h\\u1ecfng"'),
        ('Keyword = "Kiem tra dinh ky"', 'Keyword = "Ki\\u1ec3m tra \\u0111\\u1ecbnh k\\u1ef3"'),
        ('Keyword = "Yeu cau khac"', 'Keyword = "Y\\u00eau c\\u1ea7u kh\\u00e1c"'),
        ('new TaskType { Name = "Bao tri" }', 'new TaskType { Name = "B\\u1ea3o tr\\u00ec" }'),
        ('new TaskType { Name = "Sua chua" }', 'new TaskType { Name = "S\\u1eeda ch\\u1eefa" }'),
        ('new TaskType { Name = "Kiem dinh" }', 'new TaskType { Name = "Ki\\u1ec3m \\u0111\\u1ecbnh" }'),
        ('Name = "Khoa Noi"', 'Name = "Khoa N\\u1ed9i"'),
        ('Name = "Khoa Cap cuu"', 'Name = "Khoa C\\u1ea5p c\\u1ee9u"'),
        ('Name = "Khoa Hoi suc"', 'Name = "Khoa H\\u1ed3i s\\u1ee9c"'),
        ('CreateUser("admin", "Admin QLTBYT"', 'CreateUser("admin", "Qu\\u1ea3n tr\\u1ecb QLTBYT"'),
        ('CreateUser("kythuat", "Ky thuat vien"', 'CreateUser("kythuat", "K\\u1ef9 thu\\u1eadt vi\\u00ean"'),
        ('CreateUser("user", "Nhan vien khoa"', 'CreateUser("user", "Nh\\u00e2n vi\\u00ean khoa"'),
    ]
    for old, new in replacements:
        text = text.replace(old, new)

    # Remove obsolete private methods if present
    if "private static async Task EnsureDemoDeviceHierarchyAsync" in text:
        idx = text.find("    private static async Task EnsureDemoDeviceHierarchyAsync")
        end = text.find("    private static UserAccount CreateUser", idx)
        if end > idx:
            text = text[:idx] + text[end:]

    if "private async Task SeedDemoAttachmentsAsync" in text:
        idx = text.find("    private async Task SeedDemoAttachmentsAsync")
        end = text.find("    private static UserAccount CreateUser", idx)
        if end > idx:
            text = text[:idx] + text[end:]

    return text


def main() -> None:
    write_text(REFRESHER, REFRESHER_CS)
    print("Wrote", REFRESHER.name)

    seeder = patch_seeder(read_text(SEEDER_PATH))
    write_text(SEEDER_PATH, seeder)
    print("Patched DevelopmentDataSeeder.cs")


if __name__ == "__main__":
    main()
