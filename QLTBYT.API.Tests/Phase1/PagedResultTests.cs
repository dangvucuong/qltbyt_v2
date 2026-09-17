using QLTBYT.API.Models.Common;

namespace QLTBYT.API.Tests.Phase1;

public class PagedResultTests
{
    [Fact]
    public void TotalPages_Rounds_Up()
    {
        var paged = new PagedResult<int>
        {
            TotalCount = 21,
            Page = 1,
            PageSize = 20,
            Items = new List<int>()
        };

        Assert.Equal(2, paged.TotalPages);
    }
}
