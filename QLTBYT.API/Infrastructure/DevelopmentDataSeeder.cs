using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using QLTBYT.API.Data;
using QLTBYT.API.Helpers;
using QLTBYT.API.Models.Domain;

namespace QLTBYT.API.Infrastructure;

public class DevelopmentDataSeeder : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<DevelopmentDataSeeder> _logger;

    public DevelopmentDataSeeder(
        IServiceProvider serviceProvider,
        IHostEnvironment environment,
        ILogger<DevelopmentDataSeeder> logger)
    {
        _serviceProvider = serviceProvider;
        _environment = environment;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_environment.IsDevelopment())
        {
            return;
        }

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await SeedAsync(context, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Development sample data seeding failed. Fix schema or clear partial seed rows.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task SeedAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        if (!await context.DeviceStatuses.AnyAsync(cancellationToken))
        {
            context.DeviceStatuses.AddRange(
                new DeviceStatus { Id = 1, Name = "\u0110ang ho\u1ea1t \u0111\u1ed9ng" },
                new DeviceStatus { Id = 2, Name = "Kh\u00f4ng ho\u1ea1t \u0111\u1ed9ng / H\u1ecfng" },
                new DeviceStatus { Id = 3, Name = "Ch\u01b0a s\u1eed d\u1ee5ng" });
        }

        await DeviceStatusLabelEnsurer.EnsureAsync(context, cancellationToken);

        if (!await context.DeviceTypes.AnyAsync(cancellationToken))
        {
            context.DeviceTypes.AddRange(
                new DeviceType { Name = "\u004d\u00e1y th\u1edf" },
                new DeviceType { Name = "\u004d\u00e1y \u0111\u1ecd" },
                new DeviceType { Name = "Thi\u1ebft b\u1ecb h\u00ecnh \u1ea3nh" });
        }

        if (!await context.OrganizationTypes.AnyAsync(cancellationToken))
        {
            context.OrganizationTypes.AddRange(
                new OrganizationType { Name = "Khoa/Ph\u00f2ng" },
                new OrganizationType { Name = "Trung t\u00e2m" },
                new OrganizationType { Name = "\u0110\u01a1n v\u1ecb ngo\u00e0i" });
        }

        if (!await context.Countries.AnyAsync(cancellationToken))
        {
            context.Countries.AddRange(
                new Country { Id = 1, Name = "Vi\u1ec7t Nam", TwoCharCode = "VN", ThreeCharCode = "VNM" },
                new Country { Id = 2, Name = "United States", TwoCharCode = "US", ThreeCharCode = "USA" },
                new Country { Id = 3, Name = "Japan", TwoCharCode = "JP", ThreeCharCode = "JPN" });
        }

        if (!await context.EventKeywords.AnyAsync(cancellationToken))
        {
            context.EventKeywords.AddRange(
                new EventKeyword { Keyword = "B\u00e1o h\u1ecfng" },
                new EventKeyword { Keyword = "Ki\u1ec3m tra \u0111\u1ecbnh k\u1ef3" },
                new EventKeyword { Keyword = "Y\u00eau c\u1ea7u kh\u00e1c" });
        }

        if (!await context.TaskTypes.AnyAsync(cancellationToken))
        {
            context.TaskTypes.AddRange(
                new TaskType { Name = "B\u1ea3o tr\u00ec" },
                new TaskType { Name = "S\u1eeda ch\u1eefa" },
                new TaskType { Name = "Ki\u1ec3m \u0111\u1ecbnh" });
        }

        await context.SaveChangesAsync(cancellationToken);

        var orgTypeId = await context.OrganizationTypes.OrderBy(t => t.Id).Select(t => t.Id).FirstAsync(cancellationToken);

        if (!await context.Organizations.AnyAsync(cancellationToken))
        {
            context.Organizations.AddRange(
                new Organization { Name = "Khoa N\u1ed9i", TypeId = orgTypeId },
                new Organization { Name = "Khoa C\u1ea5p c\u1ee9u", TypeId = orgTypeId },
                new Organization { Name = "Khoa H\u1ed3i s\u1ee9c", TypeId = orgTypeId });
            await context.SaveChangesAsync(cancellationToken);
        }

        var orgIds = await context.Organizations.OrderBy(o => o.Id).Select(o => o.Id).Take(2).ToListAsync(cancellationToken);

        if (!await context.UserAccounts.AnyAsync(u => u.Username == "admin", cancellationToken))
        {
            context.UserAccounts.AddRange(
                CreateUser("admin", "Qu\u1ea3n tr\u1ecb QLTBYT", 1, 1, null),
                CreateUser("kythuat", "K\u1ef9 thu\u1eadt vi\u00ean", 2, 0, null),
                CreateUser("user", "Nh\u00e2n vi\u00ean khoa", 3, 0, orgIds.FirstOrDefault().ToString()));
            await context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Seeded sample users: admin, kythuat, user (password: Sample@123)");
        }

        await SampleDataVietnameseRefresher.EnsureAllAsync(context, cancellationToken);
        _logger.LogInformation("Sample data refreshed (Vietnamese labels + TB-DEMO devices).");
    }

    private static UserAccount CreateUser(string username, string fullName, int loaiTk, int admin, string? maDv)
    {
        return new UserAccount
        {
            Username = username,
            Password = PasswordHasher.Hash("Sample@123"),
            FullName = fullName,
            AccountType = loaiTk,
            Admin = admin,
            OrganizationIds = maDv,
            Email = $"{username}@demo.qltbyt.local"
        };
    }
}