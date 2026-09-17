using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using QLTBYT.API.Models.Common;
using QLTBYT.API.Models.DTOs.Auth;
using QLTBYT.API.Models.DTOs.Dashboard;
using QLTBYT.API.Models.DTOs.Device;
using QLTBYT.API.Tests.Infrastructure;

namespace QLTBYT.API.Tests.Phase4;

[Collection("DatabaseIntegration")]
[Trait("Category", "Database")]
public class GapClosureIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public GapClosureIntegrationTests(WebApplicationFactory<Program> factory)
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
    public async Task Dashboard_Summary_Includes_ByOrganization()
    {
        var client = await CreateClientAsync("admin");
        var summary = await client.GetFromJsonAsync<ApiResponse<DashboardSummaryDto>>(
            "/api/dashboard/summary?expiringWithinDays=90");
        Assert.True(summary!.Success);
        Assert.NotNull(summary.Data!.ByOrganization);
        Assert.True(summary.Data.ByOrganization.Count >= 0);
    }

    [Fact]
    public async Task Technician_Can_Use_Device_Report_Grid_And_Warehouse_Export()
    {
        var client = await CreateClientAsync("kythuat");
        var grid = await client.GetFromJsonAsync<ApiResponse<PagedResult<DeviceReportRowDto>>>(
            "/api/reports/devices/grid?page=1&pageSize=5");
        Assert.True(grid!.Success);
        Assert.NotNull(grid.Data!.Items);

        var export = await client.GetAsync("/api/reports/warehouse/export");
        Assert.Equal(HttpStatusCode.OK, export.StatusCode);
        Assert.True(export.Content.Headers.ContentType?.MediaType?.Contains("spreadsheet") == true
            || export.Content.Headers.ContentLength > 100);
    }

    [Fact]
    public async Task Technician_Can_Get_Device_Print_Html()
    {
        var client = await CreateClientAsync("kythuat");
        var grid = await client.GetFromJsonAsync<ApiResponse<PagedResult<DeviceReportRowDto>>>(
            "/api/reports/devices/grid?page=1&pageSize=1");
        var id = grid!.Data!.Items.FirstOrDefault()?.Id ?? 0;
        if (id <= 0) return;

        var repair = await client.GetAsync($"/api/reports/devices/{id}/print/repair");
        Assert.Equal(HttpStatusCode.OK, repair.StatusCode);
        var html = await repair.Content.ReadAsStringAsync();
        Assert.Contains("<html", html, StringComparison.OrdinalIgnoreCase);
    }
}