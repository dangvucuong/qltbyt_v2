using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using QLTBYT.API.Data;
using QLTBYT.API.Helpers;
using QLTBYT.API.Models.Common;
using QLTBYT.API.Models.DTOs.Auth;
using QLTBYT.API.Models.DTOs.Device;
using QLTBYT.API.Tests.Infrastructure;

namespace QLTBYT.API.Tests.Phase1;

[Collection("DatabaseIntegration")]
[Trait("Category", "Database")]
public class DeviceOrganizationScopeIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public DeviceOrganizationScopeIntegrationTests(WebApplicationFactory<Program> factory)
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
    public async Task User_Role_Sees_Only_Assigned_Organization_Devices()
    {
        var admin = await CreateClientAsync("admin");
        var adminList = await admin.GetFromJsonAsync<ApiResponse<PagedResult<DeviceListDto>>>(
            "/api/devices?pageSize=50");
        var demo002 = adminList!.Data!.Items.FirstOrDefault(d => d.DeviceCode == "TB-DEMO-002");
        Assert.NotNull(demo002);

        var userClient = await CreateClientAsync("user");
        var me = await userClient.GetFromJsonAsync<ApiResponse<UserInfoDto>>("/api/auth/me");
        var allowed = OrganizationScopeHelper.ParseOrganizationIds(me!.Data!.OrganizationIds);

        var userList = await userClient.GetFromJsonAsync<ApiResponse<PagedResult<DeviceListDto>>>(
            "/api/devices?pageSize=50");
        Assert.NotNull(userList?.Data);
        Assert.All(userList.Data.Items, d =>
            Assert.True(d.OrganizationId.HasValue && allowed.Contains(d.OrganizationId.Value)));

        var getOther = await userClient.GetAsync($"/api/devices/{demo002!.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getOther.StatusCode);
    }

    [Fact]
    public async Task Admin_Sees_All_Demo_Organizations()
    {
        var admin = await CreateClientAsync("admin");
        var list = await admin.GetFromJsonAsync<ApiResponse<PagedResult<DeviceListDto>>>(
            "/api/devices?pageSize=50&search=TB-DEMO");
        var codes = list!.Data!.Items.Select(i => i.DeviceCode).ToList();
        Assert.Contains("TB-DEMO-001", codes);
        Assert.Contains("TB-DEMO-002", codes);
    }
}
