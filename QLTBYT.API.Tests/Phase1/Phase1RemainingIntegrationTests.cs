using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using QLTBYT.API.Data;
using QLTBYT.API.Models.Common;
using QLTBYT.API.Models.DTOs.Auth;
using QLTBYT.API.Models.DTOs.Dashboard;
using QLTBYT.API.Models.DTOs.Device;
using QLTBYT.API.Models.DTOs.Master;
using QLTBYT.API.Models.DTOs.Organization;
using QLTBYT.API.Tests.Infrastructure;

namespace QLTBYT.API.Tests.Phase1;

[Collection("DatabaseIntegration")]
[Trait("Category", "Database")]
public class Phase1RemainingIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public Phase1RemainingIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:DefaultConnection", TestConnectionStrings.Resolve());
            builder.UseSetting("JwtSettings:SecretKey", "QLTBYT-TEST-Secret-Key-At-Least-32-Characters");
            builder.UseSetting("JwtSettings:Issuer", "QLTBYT-API");
            builder.UseSetting("JwtSettings:Audience", "QLTBYT-Frontend");
            builder.UseSetting(Microsoft.AspNetCore.Hosting.WebHostDefaults.EnvironmentKey, "Development");
        });

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(TestConnectionStrings.Resolve())
            .Options;
        using var context = new AppDbContext(options);
        DemoSampleDataEnsurer.EnsureSampleUserPasswordsAsync(context).GetAwaiter().GetResult();
        DemoSampleDataEnsurer.EnsureDeviceStatusLabelsAsync(context).GetAwaiter().GetResult();
        DemoSampleDataEnsurer.EnsureDemoDeviceHierarchyAsync(context).GetAwaiter().GetResult();
    }

    private async Task<HttpClient> AuthClientAsync()
    {
        var client = _factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Username = "admin",
            Password = "Sample@123"
        });
        var body = await login.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", body!.Data!.Token);
        return client;
    }

    [Fact]
    public async Task Master_And_Organizations_Return_Data()
    {
        var client = await AuthClientAsync();
        var types = await client.GetFromJsonAsync<ApiResponse<List<MasterItemDto>>>("/api/master/device-types");
        var orgs = await client.GetFromJsonAsync<ApiResponse<List<OrganizationListDto>>>("/api/organizations");
        Assert.NotNull(types?.Data);
        Assert.NotEmpty(types.Data);
        Assert.NotNull(orgs?.Data);
        Assert.NotEmpty(orgs.Data);
    }

    [Fact]
    public async Task Dashboard_Summary_Returns_Charts()
    {
        var client = await AuthClientAsync();
        var summary = await client.GetFromJsonAsync<ApiResponse<DashboardSummaryDto>>(
            "/api/dashboard/summary?expiringWithinDays=90");
        Assert.NotNull(summary?.Data);
        Assert.True(summary.Data.TotalDevices >= 3);
    }

    [Fact]
    public async Task Update_Device_And_Get_Children_Works()
    {
        var client = await AuthClientAsync();
        var list = await client.GetFromJsonAsync<ApiResponse<PagedResult<DeviceListDto>>>(
            "/api/devices?search=TB-DEMO-001");
        var device = list!.Data!.Items.First(d => d.DeviceCode == "TB-DEMO-001");

        var put = await client.PutAsJsonAsync($"/api/devices/{device.Id}", new UpdateDeviceRequest
        {
            Name = "Máy thở oxy demo (đã cập nhật)",
            TypeId = device.TypeId ?? 1,
            OrganizationId = device.OrganizationId ?? 1,
            StatusId = device.StatusId ?? 1
        });
        Assert.Equal(HttpStatusCode.OK, put.StatusCode);

        var children = await client.GetFromJsonAsync<ApiResponse<List<DeviceListDto>>>(
            $"/api/devices/{device.Id}/children");
        Assert.NotNull(children?.Data);
        Assert.Contains(children.Data, c => c.DeviceCode == "TB-DEMO-003");
    }
}
