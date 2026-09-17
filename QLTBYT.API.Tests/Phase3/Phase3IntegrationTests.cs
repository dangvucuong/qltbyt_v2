using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using QLTBYT.API.Models.Common;
using QLTBYT.API.Models.DTOs.Auth;
using QLTBYT.API.Models.DTOs.Bidding;
using QLTBYT.API.Models.DTOs.Device;
using QLTBYT.API.Models.DTOs.Organization;
using QLTBYT.API.Models.DTOs.Legal;
using QLTBYT.API.Tests.Infrastructure;

namespace QLTBYT.API.Tests.Phase3;

[Collection("DatabaseIntegration")]
[Trait("Category", "Database")]
public class Phase3IntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public Phase3IntegrationTests(WebApplicationFactory<Program> factory)
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
    public async Task Admin_Can_Create_Organization_And_Bidding_Package()
    {
        var client = await CreateClientAsync("admin");
        var createOrg = await client.PostAsJsonAsync("/api/organizations", new CreateOrganizationRequest
        {
            Name = "Khoa Demo Phase 3",
            Phone = "0900000000"
        });
        Assert.Equal(HttpStatusCode.OK, createOrg.StatusCode);

        var createPkg = await client.PostAsJsonAsync("/api/bidding", new CreateBiddingPackageRequest
        {
            PackageCode = "GT-TEST-PHASE3",
            Name = "Goi thau test phase 3",
            Status = "Dang thuc hien"
        });
        Assert.Equal(HttpStatusCode.OK, createPkg.StatusCode);
        var pkg = await createPkg.Content.ReadFromJsonAsync<ApiResponse<BiddingPackageDto>>();
        Assert.True(pkg!.Success);

        var list = await client.GetFromJsonAsync<ApiResponse<List<BiddingPackageDto>>>("/api/bidding");
        Assert.True(list!.Data!.Any(b => b.PackageCode == "GT-TEST-PHASE3"));
    }

    [Fact]
    public async Task Technician_Can_Get_Report_Summary_And_Device_History()
    {
        var client = await CreateClientAsync("kythuat");
        var summary = await client.GetFromJsonAsync<ApiResponse<DeviceReportSummaryDto>>(
            "/api/reports/devices/summary?expiringWithinDays=30");
        Assert.True(summary!.Success);
        Assert.True(summary.Data!.TotalDevices >= 1);

        var devices = await client.GetFromJsonAsync<ApiResponse<PagedResult<DeviceListDto>>>(
            "/api/devices?search=TB-DEMO-001");
        var deviceId = devices!.Data!.Items.First().Id;

        var history = await client.GetFromJsonAsync<ApiResponse<List<DeviceHistoryItemDto>>>(
            $"/api/devices/{deviceId}/history");
        Assert.True(history!.Success);
    }

    [Fact]
    public async Task Export_Devices_Excel_Returns_File()
    {
        var client = await CreateClientAsync("kythuat");
        var res = await client.GetAsync("/api/reports/devices/export");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            res.Content.Headers.ContentType?.MediaType);
        Assert.True(res.Content.Headers.ContentLength > 0);
    }

    [Fact]
    public async Task Technician_Can_Read_Legal_Document_For_Demo_Device()
    {
        var client = await CreateClientAsync("kythuat");
        var devices = await client.GetFromJsonAsync<ApiResponse<PagedResult<DeviceListDto>>>(
            "/api/devices?search=TB-DEMO-001");
        var deviceId = devices!.Data!.Items.First().Id;

        var legal = await client.GetFromJsonAsync<ApiResponse<LegalDocumentDto>>(
            $"/api/devices/{deviceId}/legal-document");
        Assert.True(legal!.Success);
        Assert.NotNull(legal.Data!.InspectionCertificate);
    }
}
