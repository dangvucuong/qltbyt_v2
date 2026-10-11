using Microsoft.EntityFrameworkCore;
using QLTBYT.API.Data;
using QLTBYT.API.Helpers;
using QLTBYT.API.Models.Domain;
using QLTBYT.API.Models.Domain.Warehouse;

namespace QLTBYT.API.Infrastructure;

/// <summary>Dong bo danh muc + tai khoan demo + thiet bi TB-DEMO-* (tieng Viet co dau).</summary>
public static class SampleDataVietnameseRefresher
{
  private const int LegacyRealDeviceCountThreshold = 50;

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
        await EnsureDemoEventsAsync(context, cancellationToken);
        await EnsureDemoBiddingAsync(context, cancellationToken);
        await EnsureDemoLegalDocumentAsync(context, cancellationToken);
        await EnsureDemoWarehouseAsync(context, cancellationToken);
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
        CancellationToken cancellationToken) where T : class
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
            var username = user.Username ?? string.Empty;
            if (SampleUserFullNames.TryGetValue(username, out var fullName))
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
            var totalDevices = await context.Devices.CountAsync(cancellationToken);
            if (totalDevices >= LegacyRealDeviceCountThreshold)
            {
                return;
            }

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


    private static async Task EnsureDemoBiddingAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        const string code = "GT-DEMO-2024";
        var pkg = await context.BiddingPackages.FirstOrDefaultAsync(b => b.PackageCode == code, cancellationToken);
        if (pkg is null)
        {
            pkg = new BiddingPackage
            {
                PackageCode = code,
                Name = "Gói thầu mua máy thở oxy demo",
                Status = "Đang thực hiện",
                WinningVendor = "Công ty Thiết bị Y tế Demo",
                TaxCode = "0123456789",
                ContractNumber = "HD-DEMO-2024-01",
                Value = 1500000000
            };
            context.BiddingPackages.Add(pkg);
            await context.SaveChangesAsync(cancellationToken);
        }

        var devices = await context.Devices
            .Where(d => d.DeviceCode != null && d.DeviceCode.StartsWith("TB-DEMO-"))
            .ToListAsync(cancellationToken);
        foreach (var d in devices)
        {
            d.BiddingPackageCode = code;
        }
        if (devices.Count > 0)
            await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task EnsureDemoLegalDocumentAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        var device = await context.Devices.FirstOrDefaultAsync(d => d.DeviceCode == "TB-DEMO-001", cancellationToken);
        if (device is null)
            return;

        var existing = await context.LegalDocuments
            .FirstOrDefaultAsync(l => l.DeviceCodes != null && l.DeviceCodes.Contains("TB-DEMO-001"), cancellationToken);
        if (existing is null)
        {
            existing = new LegalDocument
            {
                InspectionCertificate = "KD-DEMO-2024-001",
                ImportPermit = "NK-DEMO-2024-001",
                CirculationPermit = "LHH-DEMO-2024-001",
                BiddingInfo = "G\u00f3i th\u1ea7u mua m\u00e1y th\u1edf oxy demo GT-DEMO-2024",
                PriceWithVat = 125000000,
                DeviceCodes = "TB-DEMO-001"
            };
            context.LegalDocuments.Add(existing);
            await context.SaveChangesAsync(cancellationToken);
        }

        device.LegalDocumentRef = existing.Id.ToString();
        await context.SaveChangesAsync(cancellationToken);
    }



    private static async Task EnsureDemoWarehouseAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        const string code = "VT-DEMO-001";
        var commodity = await context.MedicineCommodities.FirstOrDefaultAsync(c => c.Code == code, cancellationToken);
        if (commodity is null)
        {
            var nextC = await context.MedicineCommodities.MaxAsync(c => (int?)c.Id) ?? 0;
            commodity = new MedicineCommodity
            {
                Id = nextC + 1,
                Code = code,
                Name = "Kim tiêm vật tư y tế (demo)",
                ActiveIngredient = "Vat lieu y te",
                CreatedAt = DateTime.UtcNow
            };
            context.MedicineCommodities.Add(commodity);
            await context.SaveChangesAsync(cancellationToken);
        }

        var batch = await context.MedicineBatches.FirstOrDefaultAsync(b => b.BatchCode == "LO-DEMO-2024", cancellationToken);
        if (batch is null)
        {
            var nextB = await context.MedicineBatches.MaxAsync(b => (int?)b.Id) ?? 0;
            batch = new MedicineBatch
            {
                Id = nextB + 1,
                BatchCode = "LO-DEMO-2024",
                CreatedAt = DateTime.UtcNow
            };
            context.MedicineBatches.Add(batch);
            await context.SaveChangesAsync(cancellationToken);
        }

        var hasImport = await context.StockImports.AnyAsync(i => i.CommodityId == commodity.Id, cancellationToken);
        if (!hasImport)
        {
            var nextI = await context.StockImports.MaxAsync(i => (int?)i.Id) ?? 0;
            context.StockImports.Add(new StockImport
            {
                Id = nextI + 1,
                CommodityId = commodity.Id,
                BatchId = batch.Id,
                Quantity = 500,
                StockBalance = 500,
                ExpiryDate = DateTime.Today.AddYears(2),
                CreatedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync(cancellationToken);
        }
    }
    private static async Task EnsureDemoEventsAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        var demoIds = await context.Devices
            .Where(d => d.DeviceCode != null && d.DeviceCode.StartsWith("TB-DEMO-"))
            .Select(d => d.Id)
            .ToListAsync(cancellationToken);
        if (demoIds.Count == 0)
            return;

        var old = await context.DeviceEvents
            .Where(e => e.DeviceId != null && demoIds.Contains(e.DeviceId.Value))
            .ToListAsync(cancellationToken);
        context.DeviceEvents.RemoveRange(old);

        var device002 = await context.Devices
            .FirstOrDefaultAsync(d => d.DeviceCode == "TB-DEMO-002", cancellationToken);
        if (device002 is null)
        {
            await context.SaveChangesAsync(cancellationToken);
            return;
        }

        context.DeviceEvents.Add(new DeviceEvent
        {
            DeviceId = device002.Id,
            TypeId = 1,
            Content = "Máy đo huyết áp không hoạt động - yêu cầu kiểm tra demo",
            EventDateTime = DateTime.Now.AddHours(-2),
            StatusId = 1,
            AccountUsername = "user",
            Keyword = "Bao hong"
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
