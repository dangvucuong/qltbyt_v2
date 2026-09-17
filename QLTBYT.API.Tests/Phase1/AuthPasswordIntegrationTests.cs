using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using QLTBYT.API.Data;
using QLTBYT.API.Models.Common;
using QLTBYT.API.Models.DTOs.Auth;
using QLTBYT.API.Tests.Infrastructure;

namespace QLTBYT.API.Tests.Phase1;

[Collection("DatabaseIntegration")]
[Trait("Category", "Database")]
public class AuthPasswordIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AuthPasswordIntegrationTests(WebApplicationFactory<Program> factory)
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

    [Fact]
    public async Task ChangePassword_Then_Login_With_New_Password_Works()
    {
        var client = _factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Username = "kythuat",
            Password = "Sample@123"
        });
        var loginBody = await login.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginBody!.Data!.Token);

        var newPassword = "Sample@456";
        var change = await client.PostAsJsonAsync("/api/auth/change-password", new ChangePasswordRequest
        {
            CurrentPassword = "Sample@123",
            NewPassword = newPassword
        });
        Assert.Equal(HttpStatusCode.OK, change.StatusCode);

        var loginNew = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Username = "kythuat",
            Password = newPassword
        });
        Assert.Equal(HttpStatusCode.OK, loginNew.StatusCode);

        // restore for other tests
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginBody.Data.Token);
        await client.PostAsJsonAsync("/api/auth/change-password", new ChangePasswordRequest
        {
            CurrentPassword = newPassword,
            NewPassword = "Sample@123"
        });
    }

    [Fact]
    public async Task Forgot_And_Reset_Password_Works_In_Development()
    {
        var client = _factory.CreateClient();
        var forgot = await client.PostAsJsonAsync("/api/auth/forgot-password", new ForgotPasswordRequest
        {
            Username = "user"
        });
        Assert.Equal(HttpStatusCode.OK, forgot.StatusCode);
        var forgotBody = await forgot.Content.ReadFromJsonAsync<ApiResponse<ForgotPasswordResponse>>();
        Assert.NotNull(forgotBody?.Data?.ResetToken);

        var newPassword = "Sample@789";
        var reset = await client.PostAsJsonAsync("/api/auth/reset-password", new ResetPasswordRequest
        {
            Username = "user",
            ResetToken = forgotBody!.Data!.ResetToken!,
            NewPassword = newPassword
        });
        Assert.Equal(HttpStatusCode.OK, reset.StatusCode);

        var login = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Username = "user",
            Password = newPassword
        });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);

        var loginBody = await login.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginBody!.Data!.Token);
        await client.PostAsJsonAsync("/api/auth/change-password", new ChangePasswordRequest
        {
            CurrentPassword = newPassword,
            NewPassword = "Sample@123"
        });
    }
}
