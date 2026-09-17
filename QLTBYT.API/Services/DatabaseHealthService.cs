using Microsoft.Data.SqlClient;
using QLTBYT.API.Models.DTOs.Health;

namespace QLTBYT.API.Services;

public class DatabaseHealthService : IDatabaseHealthService
{
    private static readonly string[] CoreTables = { "Quanlythietbi", "Taikhoan", "QuanlyDV", "LogThietbi", "ThongtinHS", "Goithau" };
    private readonly string _connectionString;
    public DatabaseHealthService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string DefaultConnection is not configured.");
    }
    public async Task<DatabaseHealthDto> CheckAsync()
    {
        var result = new DatabaseHealthDto { DatabaseName = GetDatabaseNameFromConnectionString(_connectionString) };
        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            result.CanConnect = true;
            var existingTables = await GetTableNamesAsync(connection);
            result.TableCount = existingTables.Count;
            var missing = CoreTables.Where(t => !existingTables.Contains(t, StringComparer.OrdinalIgnoreCase)).ToList();
            result.MissingCoreTables = missing;
            result.CoreTablesPresent = missing.Count == 0;
        }
        catch
        {
            result.CanConnect = false;
            result.CoreTablesPresent = false;
            result.MissingCoreTables = CoreTables;
        }
        return result;
    }
    private static string GetDatabaseNameFromConnectionString(string connectionString)
    {
        var builder = new SqlConnectionStringBuilder(connectionString);
        return builder.InitialCatalog ?? string.Empty;
    }
    private static async Task<HashSet<string>> GetTableNamesAsync(SqlConnection connection)
    {
        const string sql = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'";
        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();
        var tables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        while (await reader.ReadAsync()) { tables.Add(reader.GetString(0)); }
        return tables;
    }
}
