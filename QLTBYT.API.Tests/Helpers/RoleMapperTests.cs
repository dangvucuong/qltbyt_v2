using QLTBYT.API.Helpers;
using QLTBYT.API.Models.Domain;
using Xunit;

namespace QLTBYT.API.Tests.Helpers;

public class RoleMapperTests
{
    [Theory]
    [InlineData(0, 1, "Admin")]   // legacy admin
    [InlineData(1, 0, "Admin")]   // legacy LoaiTK 1
    [InlineData(1, 1, "Admin")]   // seed admin
    [InlineData(2, 0, "Technician")]
    [InlineData(3, 0, "User")]
    [InlineData(3, 1, "Admin")]   // Admin flag overrides LoaiTK 3
    public void ResolveRole_Maps_Legacy_And_Seed(int loaiTk, int admin, string expected)
    {
        var account = new UserAccount { AccountType = loaiTk, Admin = admin };
        Assert.Equal(expected, RoleMapper.ResolveRole(account));
    }
}
