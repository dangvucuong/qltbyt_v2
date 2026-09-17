using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc.Testing;
using QLTBYT.API.Models.Common;
using QLTBYT.API.Models.DTOs.Auth;
using QLTBYT.API.Tests.Infrastructure;

namespace QLTBYT.API.Tests.Phase1;

[Collection("DatabaseIntegration")]
[Trait("Category", "Database")]
public class AuthControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AuthControllerIntegrationTests(WebApplicationFactory<Program> factory)
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

    [Fact]
    public async Task Login_With_Sample_Admin_Returns_Token()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Username = "admin",
            Password = "Sample@123"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();
        Assert.NotNull(body);
        Assert.True(body.Success);
        Assert.NotNull(body.Data);
        Assert.False(string.IsNullOrWhiteSpace(body.Data.Token));
        Assert.Equal("admin", body.Data.User.Username);
        Assert.Equal("Admin", body.Data.User.Role);
    }

    [Fact]
    public async Task Login_With_Wrong_Password_Returns_401()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Username = "admin",
            Password = "wrong-password"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_With_Valid_Token_Returns_User()
    {
        var client = _factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Username = "kythuat",
            Password = "Sample@123"
        });
        var loginBody = await login.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();
        Assert.NotNull(loginBody?.Data?.Token);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginBody.Data.Token);

        var meResponse = await client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);
        var me = await meResponse.Content.ReadFromJsonAsync<ApiResponse<UserInfoDto>>();
        Assert.NotNull(me?.Data);
        Assert.Equal("kythuat", me.Data.Username);
        Assert.Equal("Technician", me.Data.Role);
    }
}
