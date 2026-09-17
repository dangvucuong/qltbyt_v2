using Microsoft.Extensions.Configuration;

namespace QLTBYT.API.Tests.Infrastructure;

public static class TestConnectionStrings
{
    public static string Resolve()
    {
        var fromEnv = Environment.GetEnvironmentVariable("QLTBYT_CONNECTION");
        if (!string.IsNullOrWhiteSpace(fromEnv))
        {
            return fromEnv;
        }

        var apiSettingsPath = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "QLTBYT.API", "appsettings.Development.json"));

        if (!File.Exists(apiSettingsPath))
        {
            throw new InvalidOperationException(
                "Thiết lập QLTBYT_CONNECTION hoặc file QLTBYT.API/appsettings.Development.json cho test database.");
        }

        var configuration = new ConfigurationBuilder()
            .AddJsonFile(apiSettingsPath, optional: false)
            .Build();

        return configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
    }
}
