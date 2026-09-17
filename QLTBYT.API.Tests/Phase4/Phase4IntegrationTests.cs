using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using QLTBYT.API.Models.Common;
using QLTBYT.API.Models.DTOs.Auth;
using QLTBYT.API.Models.DTOs.Warehouse;
using QLTBYT.API.Tests.Infrastructure;

namespace QLTBYT.API.Tests.Phase4;

[Collection("DatabaseIntegration")]
[Trait("Category", "Database")]
public class Phase4IntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public Phase4IntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:DefaultConnection", TestConnectionStrings.Resolve());
            builder.UseSetting("JwtSettings:SecretKey", "QLTBYT-TEST-Secret-Key-At-Least-32-Characters");
            builder.UseSetting("JwtSettings:Issuer", "QLTBYT-API");
            builder.UseSetting("JwtSettings:Audience", "QLTBYT-Frontend");
            builder.UseSetting(Microsoft.AspNetCore.Hosting.WebHostDefaults.EnvironmentKey, "Development");
        });
    }

    private async Task<HttpClient> CreateClientAsync(string username)
    {
        var client = _factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Username = username,
            Password = "Sample@123"
        });
        var body = await login.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", body!.Data!.Token);
        return client;
    }

    [Fact]
    public async Task Technician_Can_List_Warehouse_Commodities_And_Report()
    {
        var client = await CreateClientAsync("kythuat");
        var commodities = await client.GetFromJsonAsync<ApiResponse<List<CommodityDto>>>("/api/warehouse/commodities");
        Assert.True(commodities!.Success);
        Assert.True(commodities.Data!.Any(c => c.Code == "VT-DEMO-001"));

        var report = await client.GetFromJsonAsync<ApiResponse<WarehouseReportDto>>("/api/warehouse/report");
        Assert.True(report!.Success);
        Assert.True(report.Data!.TotalCommodities >= 1);
        Assert.True(report.Data.TotalStockUnits >= 500);
    }

    [Fact]
    public async Task Technician_Can_Import_And_Export_Warehouse_Stock()
    {
        var client = await CreateClientAsync("kythuat");
        var create = await client.PostAsJsonAsync("/api/warehouse/commodities", new CreateCommodityRequest
        {
            Code = "VT-TEST-PHASE4",
            Name = "Gang tay y te test",
            ActiveIngredient = "PVC"
        });
        Assert.Equal(HttpStatusCode.OK, create.StatusCode);
        var commodity = await create.Content.ReadFromJsonAsync<ApiResponse<CommodityDto>>();

        var batch = await client.PostAsJsonAsync("/api/warehouse/batches", new CreateBatchRequest
        {
            BatchCode = "LO-TEST-PHASE4"
        });
        Assert.Equal(HttpStatusCode.OK, batch.StatusCode);
        var batchBody = await batch.Content.ReadFromJsonAsync<ApiResponse<BatchDto>>();

        var import = await client.PostAsJsonAsync("/api/warehouse/imports", new CreateStockImportRequest
        {
            CommodityId = commodity!.Data!.Id,
            BatchId = batchBody!.Data!.Id,
            Quantity = 20,
            ExpiryDate = DateTime.Today.AddYears(1)
        });
        Assert.Equal(HttpStatusCode.OK, import.StatusCode);
        var importBody = await import.Content.ReadFromJsonAsync<ApiResponse<StockImportDto>>();

        var export = await client.PostAsJsonAsync("/api/warehouse/exports", new CreateStockExportRequest
        {
            ImportId = importBody!.Data!.Id,
            Quantity = 5,
            Note = "Xuat demo phase 4"
        });
        Assert.Equal(HttpStatusCode.OK, export.StatusCode);
    }
}