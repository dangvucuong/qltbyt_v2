using Microsoft.Data.SqlClient;
using QLTBYT.API.Tests.Infrastructure;

namespace QLTBYT.API.Tests.Step0;

[Trait("Category", "Database")]
public class DatabaseSchemaTests
{
  private static readonly string[] ExpectedCoreTables =
    {
        "Quanlythietbi",
        "Taikhoan",
        "QuanlyDV",
        "LogThietbi",
        "ThongtinHS",
        "Goithau",
        "Quanlyfile",
        "QuanlyMa",
        "QuanlyNhansu",
        "Quanlyvattu",
        "InputByDate",
        "LogHethong",
        "Thongbao",
        "Thongbaostatus",
        "EventKeyword",
        "DMLoaiTB",
        "DMTrangthai",
        "DanhmucDV",
        "Country"
    };

    [Fact]
    public async Task Database_db_QLTBYT_NEW_Should_Connect()
    {
        await using var connection = new SqlConnection(TestConnectionStrings.Resolve());
        await connection.OpenAsync();
        Assert.Equal("db_QLTBYT_NEW", connection.Database);
    }

    [Fact]
    public async Task Database_Should_Have_At_Least_30_Base_Tables()
    {
        await using var connection = new SqlConnection(TestConnectionStrings.Resolve());
        await connection.OpenAsync();

        const string sql =
            "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'";

        await using var command = new SqlCommand(sql, connection);
        var count = (int)(await command.ExecuteScalarAsync() ?? 0);

        Assert.True(count >= 30, $"Expected >= 30 tables, got {count}.");
    }

    [Fact]
    public async Task Database_Should_Contain_All_Core_Tables()
    {
        await using var connection = new SqlConnection(TestConnectionStrings.Resolve());
        await connection.OpenAsync();

        var tables = await GetTableNamesAsync(connection);
        var missing = ExpectedCoreTables.Where(t => !tables.Contains(t, StringComparer.OrdinalIgnoreCase)).ToList();

        Assert.Empty(missing);
    }

    private static async Task<HashSet<string>> GetTableNamesAsync(SqlConnection connection)
    {
        const string sql =
            "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'";

        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        var tables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        while (await reader.ReadAsync())
        {
            tables.Add(reader.GetString(0));
        }

        return tables;
    }
}
