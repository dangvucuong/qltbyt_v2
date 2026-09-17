using QLTBYT.API.Helpers;

namespace QLTBYT.API.Tests.Phase1;

public class OrganizationScopeHelperTests
{
    [Fact]
    public void ParseOrganizationIds_Splits_Semicolon_List()
    {
        var ids = OrganizationScopeHelper.ParseOrganizationIds("1; 2;3");
        Assert.Equal(new List<int> { 1, 2, 3 }, ids);
    }

    [Fact]
    public void IsOrganizationAllowed_Respects_List()
    {
        var allowed = new List<int> { 5, 10 };
        Assert.True(OrganizationScopeHelper.IsOrganizationAllowed(5, allowed));
        Assert.False(OrganizationScopeHelper.IsOrganizationAllowed(7, allowed));
    }
}
