using Microsoft.Extensions.Configuration;
using QLTBYT.API.Services;
using QLTBYT.API.Tests.Infrastructure;

namespace QLTBYT.API.Tests.Step0;

[Trait("Category", "Database")]
public class DatabaseHealthServiceTests
{
    [Fact]
    public async Task CheckAsync_WhenDatabaseReady_ReturnsHealthy()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = TestConnectionStrings.Resolve()
            })
            .Build();

        var service = new DatabaseHealthService(configuration);
        var result = await service.CheckAsync();

        Assert.True(result.CanConnect);
        Assert.Equal("db_QLTBYT_NEW", result.DatabaseName);
        Assert.True(result.TableCount >= 30);
        Assert.True(result.CoreTablesPresent);
        Assert.Empty(result.MissingCoreTables);
    }
}
