using QLTBYT.API.Helpers;
using QLTBYT.API.Models.Domain;
using Xunit;

namespace QLTBYT.API.Tests.Helpers;

public class RoleMapperTests
{
    [Theory]
    [InlineData(0, 0, "Admin")]       // Ban lãnh đạo
    [InlineData(0, 1, "Admin")]
    [InlineData(1, 0, "Technician")]  // Tổ kho, thiết bị
    [InlineData(2, 0, "User")]        // Kỹ sư khu vực
    [InlineData(3, 0, "User")]        // Nhân viên mạng lưới
    [InlineData(3, 1, "Admin")]       // Admin flag overrides
    public void ResolveRole_Maps_Legacy_LoaiTK(int loaiTk, int admin, string expected)
    {
        var account = new UserAccount { AccountType = loaiTk, Admin = admin };
        Assert.Equal(expected, RoleMapper.ResolveRole(account));
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(1, true)]
    [InlineData(2, true)]
    [InlineData(3, false)]
    public void CanAccessReports_Matches_Legacy_Baocao(int loaiTk, bool expected)
    {
        Assert.Equal(expected, RoleMapper.CanAccessReports(loaiTk));
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(1, true)]
    [InlineData(2, false)]
    [InlineData(3, false)]
    public void CanAccessBidding_Matches_Legacy_Goithau(int loaiTk, bool expected)
    {
        Assert.Equal(expected, RoleMapper.CanAccessBidding(loaiTk));
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(1, false)]
    [InlineData(2, false)]
    [InlineData(3, true)]
    public void CanAccessWarehouse_Matches_Legacy_Kho(int loaiTk, bool expected)
    {
        Assert.Equal(expected, RoleMapper.CanAccessWarehouse(loaiTk));
    }
}
