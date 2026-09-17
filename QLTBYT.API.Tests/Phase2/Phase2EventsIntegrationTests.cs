using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using QLTBYT.API.Models.Common;
using QLTBYT.API.Models.DTOs.Device;
using QLTBYT.API.Models.DTOs.Event;
using QLTBYT.API.Models.DTOs.Auth;
using QLTBYT.API.Tests.Infrastructure;

namespace QLTBYT.API.Tests.Phase2;

[Collection("DatabaseIntegration")]
[Trait("Category", "Database")]
public class Phase2EventsIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public Phase2EventsIntegrationTests(WebApplicationFactory<Program> factory)
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
    public async Task User_Can_Create_Event_And_Technician_Can_Resolve()
    {
        var userClient = await CreateClientAsync("user");
        var devices = await userClient.GetFromJsonAsync<ApiResponse<PagedResult<DeviceListDto>>>(
            "/api/devices?search=TB-DEMO-001");
        var deviceId = devices!.Data!.Items.First().Id;

        var create = await userClient.PostAsJsonAsync($"/api/devices/{deviceId}/events", new CreateDeviceEventRequest
        {
            EventTypeId = 1,
            Content = "Test su co phase 2",
            Keyword = "Bao hong"
        });
        Assert.Equal(HttpStatusCode.OK, create.StatusCode);
        var created = await create.Content.ReadFromJsonAsync<ApiResponse<EventDetailDto>>();
        Assert.True(created!.Success);
        var eventId = created.Data!.Id;

        var techClient = await CreateClientAsync("kythuat");
        var list = await techClient.GetFromJsonAsync<ApiResponse<PagedResult<EventListDto>>>("/api/events?statusId=1");
        Assert.True(list!.Data!.TotalCount >= 1);

        var resolve = await techClient.PutAsJsonAsync($"/api/events/{eventId}", new ResolveEventRequest
        {
            ResolutionContent = "Da xu ly test",
            Cost = 0
        });
        Assert.Equal(HttpStatusCode.OK, resolve.StatusCode);
    }
}
