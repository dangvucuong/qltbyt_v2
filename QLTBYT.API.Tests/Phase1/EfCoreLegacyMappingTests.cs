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
        await DemoSampleDataEnsurer.EnsureDemoDeviceAttachmentsAsync(context);

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
        await DemoSampleDataEnsurer.EnsureDemoDeviceAttachmentsAsync(context);

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
        await DemoSampleDataEnsurer.EnsureMasterDataAsync(context);

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
        await DemoSampleDataEnsurer.EnsureMasterDataAsync(context);

        var orgs = await context.Organizations
            .Include(o => o.OrganizationType)
            .OrderBy(o => o.Id)
            .Take(5)
            .ToListAsync();

        Assert.NotEmpty(orgs);
    }
}