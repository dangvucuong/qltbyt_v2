using Microsoft.EntityFrameworkCore;
using QLTBYT.API.Data;

namespace QLTBYT.API.Infrastructure;

/// <summary>One-time fixes for legacy SQL schema (e.g. Taikhoan.Matkhau too short for BCrypt).</summary>
public static class LegacySchemaEnsurer
{
    /// <summary>BCrypt hashes need 60 characters; legacy column was nvarchar(50).</summary>
    private const int MinMatkhauColLengthBytes = 200;

    public static async Task EnsureMatkhauForBcryptAsync(AppDbContext context, CancellationToken cancellationToken = default)
    {
        const string sql =
            "IF COL_LENGTH('Taikhoan', 'Matkhau') IS NOT NULL " +
            "AND COL_LENGTH('Taikhoan', 'Matkhau') < {0} " +
            "ALTER TABLE Taikhoan ALTER COLUMN Matkhau NVARCHAR(200) NULL;";

        await context.Database.ExecuteSqlRawAsync(sql, new object[] { MinMatkhauColLengthBytes }, cancellationToken);
    }
}
