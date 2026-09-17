using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using QLTBYT.API.Models.Common;
using QLTBYT.API.Models.DTOs.Auth;
using QLTBYT.API.Models.DTOs.Device;
using QLTBYT.API.Tests.Infrastructure;

namespace QLTBYT.API.Tests.Phase1;

[Collection("DatabaseIntegration")]
[Trait("Category", "Database")]
public class DevicesControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public DevicesControllerIntegrationTests(WebApplicationFactory<Program> factory)
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

    private async Task<HttpClient> CreateAuthenticatedClientAsync(string username = "admin")
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
    public async Task GetDevices_Returns_PagedResult_With_TotalCount()
    {
        var client = await CreateAuthenticatedClientAsync();
        var response = await client.GetAsync("/api/devices?page=1&pageSize=5&search=DEMO");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<DeviceListDto>>>();
        Assert.NotNull(body);
        Assert.True(body.Success);
        Assert.NotNull(body.Data);
        Assert.True(body.Data.TotalCount >= 3);
        Assert.NotEmpty(body.Data.Items);
        Assert.Equal(1, body.Data.Page);
        Assert.Equal(5, body.Data.PageSize);
    }

    [Fact]
    public async Task PostDevice_Invalid_Returns_400_With_Errors()
    {
        var client = await CreateAuthenticatedClientAsync();
        var response = await client.PostAsJsonAsync("/api/devices", new CreateDeviceRequest
        {
            DeviceCode = "",
            Name = "",
            TypeId = 0,
            OrganizationId = 0,
            StatusId = 0
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<DeviceDetailDto>>();
        Assert.NotNull(body);
        Assert.False(body.Success);
        Assert.NotNull(body.Errors);
        Assert.NotEmpty(body.Errors);
    }

    [Fact]
    public async Task PostDevice_Valid_Creates_Sample_Row()
    {
        var client = await CreateAuthenticatedClientAsync();
        var listResp = await client.GetAsync("/api/devices?pageSize=1");
        var listBody = await listResp.Content.ReadFromJsonAsync<ApiResponse<PagedResult<DeviceListDto>>>();
        var sample = listBody!.Data!.Items.First();
        var sampleTypeId = sample.TypeId ?? 1;
        var sampleOrgId = sample.OrganizationId ?? 1;
        var code = "TB-PHASE14-" + Guid.NewGuid().ToString("N")[..6];
        var response = await client.PostAsJsonAsync("/api/devices", new CreateDeviceRequest
        {
            DeviceCode = code,
            Name = "Thiet bi test Phase 1.4",
            TypeId = sampleTypeId,
            OrganizationId = sampleOrgId,
            StatusId = 1,
            Manufacturer = "Demo"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<DeviceDetailDto>>();
        Assert.NotNull(body?.Data);
        Assert.Equal(code, body.Data.DeviceCode);
    }
}
