using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using QLTBYT.API.Models.Common;
using QLTBYT.API.Models.DTOs.Health;
using QLTBYT.API.Tests.Infrastructure;

namespace QLTBYT.API.Tests.Step0;

[Collection("DatabaseIntegration")]
[Trait("Category", "Database")]
public class HealthControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public HealthControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:DefaultConnection", TestConnectionStrings.Resolve());
            builder.UseSetting("JwtSettings:SecretKey", "QLTBYT-TEST-Secret-Key-At-Least-32-Characters");
            builder.UseSetting("JwtSettings:Issuer", "QLTBYT-API");
            builder.UseSetting("JwtSettings:Audience", "QLTBYT-Frontend");
        });
    }

    [Fact]
    public async Task GetDatabaseHealth_Returns_Ok_When_Database_Is_Ready()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/health/database");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<DatabaseHealthDto>>();
        Assert.NotNull(body);
        Assert.True(body.Success);
        Assert.NotNull(body.Data);
        Assert.True(body.Data.CanConnect);
        Assert.True(body.Data.CoreTablesPresent);
        Assert.Equal("db_QLTBYT_NEW", body.Data.DatabaseName);
    }
}
