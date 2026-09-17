# -*- coding: utf-8 -*-
"""Write Phase 1.3 EF domain files as UTF-8 (no BOM)."""
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
API = ROOT / "QLTBYT.API"
DOMAIN = API / "Models" / "Domain"
DATA = API / "Data"
TESTS = ROOT / "QLTBYT.API.Tests" / "Phase1"

files: dict[Path, str] = {}

files[DOMAIN / "DeviceFile.cs"] = '''
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLTBYT.API.Models.Domain;

[Table("Quanlyfile")]
public class DeviceFile
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("Path")]
    public string? Path { get; set; }

    [Column("Mota")]
    public string? Description { get; set; }

    [Column("idTB")]
    public int? DeviceId { get; set; }

    [Column("Ngaythang")]
    public DateTime? FileDate { get; set; }

    public virtual Device? Device { get; set; }
}
'''.strip() + "\n"

files[DOMAIN / "DeviceCode.cs"] = '''
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLTBYT.API.Models.Domain;

[Table("QuanlyMa")]
public class DeviceCode
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("IDTB")]
    public int? DeviceId { get; set; }

    [Column("QRCODE")]
    public string? QrCode { get; set; }

    [Column("NFC")]
    public string? Nfc { get; set; }

    [Column("IDNguoiSD")]
    public int? AssignedUserId { get; set; }

    public virtual Device? Device { get; set; }
}
'''.strip() + "\n"

files[DOMAIN / "Country.cs"] = '''
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLTBYT.API.Models.Domain;

[Table("Country")]
public class Country
{
    [Key]
    [Column("CountryID")]
    public int Id { get; set; }

    [Column("CountryName")]
    public string? Name { get; set; }

    [Column("TwoCharCountryCode")]
    public string? TwoCharCode { get; set; }

    [Column("ThreeCharCountryCode")]
    public string? ThreeCharCode { get; set; }
}
'''.strip() + "\n"

files[DOMAIN / "OrganizationType.cs"] = '''
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLTBYT.API.Models.Domain;

[Table("DanhmucDV")]
public class OrganizationType
{
    [Key]
    [Column("MaloaiDV")]
    public int Id { get; set; }

    [Column("TenloaiDV")]
    public string? Name { get; set; }
}
'''.strip() + "\n"

files[DOMAIN / "EventKeyword.cs"] = '''
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLTBYT.API.Models.Domain;

[Table("EventKeyword")]
public class EventKeyword
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("Keyword")]
    public string Keyword { get; set; } = string.Empty;
}
'''.strip() + "\n"

files[DOMAIN / "TaskType.cs"] = '''
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLTBYT.API.Models.Domain;

[Table("DMTacvu")]
public class TaskType
{
    [Key]
    [Column("Matacvu")]
    public int Id { get; set; }

    [Column("Tentacvu")]
    public string? Name { get; set; }
}
'''.strip() + "\n"

files[DOMAIN / "Device.cs"] = '''
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLTBYT.API.Models.Domain;

[Table("Quanlythietbi")]
public class Device
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("MaTB")]
    public string? DeviceCode { get; set; }

    [Column("MaKT")]
    public string? TechnicalCode { get; set; }

    [Column("Madinhdanh")]
    public string? IdentityCode { get; set; }

    [Column("TenTB")]
    public string? Name { get; set; }

    [Column("Loai")]
    public int? TypeId { get; set; }

    [Column("Kyhieu")]
    public string? ModelCode { get; set; }

    [Column("Serial")]
    public string? Serial { get; set; }

    [Column("ThongsoKT")]
    public string? TechnicalSpecs { get; set; }

    [Column("Ngaytiepnhan")]
    public DateTime? ReceivedDate { get; set; }

    [Column("Donvibaotri")]
    public int? MaintenanceOrgId { get; set; }

    [Column("Donvibangiao")]
    public int? HandoverOrgId { get; set; }

    [Column("Donvisudung")]
    public int? OrganizationId { get; set; }

    [Column("Nguoiquanly")]
    public int? ManagerUserId { get; set; }

    [Column("Thoigianbh")]
    public string? WarrantyPeriod { get; set; }

    [Column("HangSX")]
    public string? Manufacturer { get; set; }

    [Column("NuocSX")]
    public string? CountryOfOrigin { get; set; }

    [Column("NamSX")]
    public string? YearOfManufacture { get; set; }

    [Column("Vattudikem")]
    public string? Accessories { get; set; }

    [Column("Trangthai")]
    public int? StatusId { get; set; }

    [Column("Tinhtrang")]
    public string? ConditionNote { get; set; }

    [Column("Ngaycapnhat")]
    public DateTime? UpdatedAt { get; set; }

    [Column("Ghichu")]
    public string? Note { get; set; }

    [Column("Thietbichinh")]
    public string? ParentDeviceCode { get; set; }

    [Column("Hankiemdinh")]
    public DateTime? InspectionExpiryDate { get; set; }

    [Column("Soluong")]
    public double? Quantity { get; set; }

    [Column("Donvicungcap")]
    public int? SupplierOrgId { get; set; }

    public virtual Organization? Organization { get; set; }
    public virtual DeviceType? DeviceType { get; set; }
    public virtual DeviceStatus? DeviceStatus { get; set; }
    public virtual ICollection<DeviceFile> Files { get; set; } = new List<DeviceFile>();
    public virtual ICollection<DeviceCode> Codes { get; set; } = new List<DeviceCode>();
}
'''.strip() + "\n"

files[DOMAIN / "Organization.cs"] = '''
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLTBYT.API.Models.Domain;

[Table("QuanlyDV")]
public class Organization
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("Tendonvi")]
    public string? Name { get; set; }

    [Column("Masothue")]
    public string? TaxCode { get; set; }

    [Column("Diachi")]
    public string? Address { get; set; }

    [Column("Dienthoai")]
    public string? Phone { get; set; }

    [Column("Email")]
    public string? Email { get; set; }

    [Column("MaLoai")]
    public int? TypeId { get; set; }

    public virtual OrganizationType? OrganizationType { get; set; }
    public virtual ICollection<Device> Devices { get; set; } = new List<Device>();
}
'''.strip() + "\n"

files[DOMAIN / "DeviceType.cs"] = '''
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLTBYT.API.Models.Domain;

[Table("DMLoaiTB")]
public class DeviceType
{
    [Key]
    [Column("MaloaiTB")]
    public int Id { get; set; }

    [Column("TenloaiTB")]
    public string? Name { get; set; }

    public virtual ICollection<Device> Devices { get; set; } = new List<Device>();
}
'''.strip() + "\n"

files[DOMAIN / "DeviceStatus.cs"] = '''
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLTBYT.API.Models.Domain;

[Table("DMTrangthai")]
public class DeviceStatus
{
    [Key]
    [Column("MaTrangthai")]
    public int Id { get; set; }

    [Column("TenTrangthai")]
    public string? Name { get; set; }

    public virtual ICollection<Device> Devices { get; set; } = new List<Device>();
}
'''.strip() + "\n"

files[DATA / "AppDbContext.cs"] = '''
using Microsoft.EntityFrameworkCore;
using QLTBYT.API.Models.Domain;

namespace QLTBYT.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<UserAccount> UserAccounts => Set<UserAccount>();
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<OrganizationType> OrganizationTypes => Set<OrganizationType>();
    public DbSet<DeviceType> DeviceTypes => Set<DeviceType>();
    public DbSet<DeviceStatus> DeviceStatuses => Set<DeviceStatus>();
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<DeviceFile> DeviceFiles => Set<DeviceFile>();
    public DbSet<DeviceCode> DeviceCodes => Set<DeviceCode>();
    public DbSet<Country> Countries => Set<Country>();
    public DbSet<EventKeyword> EventKeywords => Set<EventKeyword>();
    public DbSet<TaskType> TaskTypes => Set<TaskType>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Device>(entity =>
        {
            entity.HasOne(d => d.Organization)
                .WithMany(o => o.Devices)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(d => d.DeviceType)
                .WithMany(t => t.Devices)
                .HasForeignKey(d => d.TypeId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(d => d.DeviceStatus)
                .WithMany(s => s.Devices)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Organization>(entity =>
        {
            entity.HasOne(o => o.OrganizationType)
                .WithMany()
                .HasForeignKey(o => o.TypeId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<DeviceFile>(entity =>
        {
            entity.HasOne(f => f.Device)
                .WithMany(d => d.Files)
                .HasForeignKey(f => f.DeviceId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<DeviceCode>(entity =>
        {
            entity.HasOne(c => c.Device)
                .WithMany(d => d.Codes)
                .HasForeignKey(c => c.DeviceId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<EventKeyword>(entity =>
        {
            entity.Property(e => e.Keyword).IsFixedLength();
        });
    }
}
'''.strip() + "\n"

files[TESTS / "EfCoreLegacyMappingTests.cs"] = '''
using Microsoft.EntityFrameworkCore;
using QLTBYT.API.Data;
using QLTBYT.API.Tests.Infrastructure;

namespace QLTBYT.API.Tests.Phase1;

[Trait("Category", "Database")]
public class EfCoreLegacyMappingTests
{
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(TestConnectionStrings.Resolve())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task Devices_QueryWithIncludes_Succeeds()
    {
        await using var context = CreateContext();

        var list = await context.Devices
            .Include(d => d.Organization)
            .Include(d => d.DeviceType)
            .Include(d => d.DeviceStatus)
            .Include(d => d.Files)
            .Include(d => d.Codes)
            .OrderBy(d => d.Id)
            .Take(10)
            .ToListAsync();

        Assert.NotEmpty(list);
        Assert.All(list, d => Assert.NotNull(d.DeviceCode));
    }

    [Fact]
    public async Task Demo_Device_Has_Files_And_QrCodes()
    {
        await using var context = CreateContext();

        var device = await context.Devices
            .Include(d => d.Files)
            .Include(d => d.Codes)
            .FirstOrDefaultAsync(d => d.DeviceCode == "TB-DEMO-001");

        Assert.NotNull(device);
        Assert.NotEmpty(device!.Files);
        Assert.NotEmpty(device.Codes);
        Assert.Contains(device.Codes, c => !string.IsNullOrWhiteSpace(c.QrCode));
    }

    [Fact]
    public async Task MasterData_Countries_And_OrganizationTypes_Query_Succeeds()
    {
        await using var context = CreateContext();

        var countries = await context.Countries.OrderBy(c => c.Id).Take(5).ToListAsync();
        var orgTypes = await context.OrganizationTypes.OrderBy(t => t.Id).Take(5).ToListAsync();
        var keywords = await context.EventKeywords.Take(5).ToListAsync();
        var taskTypes = await context.TaskTypes.Take(5).ToListAsync();

        Assert.NotEmpty(countries);
        Assert.NotEmpty(orgTypes);
        Assert.NotEmpty(keywords);
        Assert.NotEmpty(taskTypes);
    }

    [Fact]
    public async Task Organizations_QueryWithType_Succeeds()
    {
        await using var context = CreateContext();

        var orgs = await context.Organizations
            .Include(o => o.OrganizationType)
            .OrderBy(o => o.Id)
            .Take(5)
            .ToListAsync();

        Assert.NotEmpty(orgs);
    }
}
'''.strip() + "\n"

for path, content in files.items():
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(content, encoding="utf-8", newline="\n")
    print("written", path.relative_to(ROOT))

print("done")
