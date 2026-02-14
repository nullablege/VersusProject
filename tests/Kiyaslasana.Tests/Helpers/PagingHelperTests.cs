using Kiyaslasana.BL.Helpers;

namespace Kiyaslasana.Tests.Helpers;

public class PagingHelperTests
{
    [Fact]
    public void Normalize_ClampsRequestedPageToLastPage()
    {
        var result = PagingHelper.Normalize(requestedPage: 999, pageSize: 48, totalCount: 100);

        Assert.Equal(3, result.Page);
        Assert.Equal(48, result.PageSize);
        Assert.Equal(3, result.TotalPages);
        Assert.Equal(96, result.Skip);
    }

    [Fact]
    public void Normalize_HandlesEmptyResultSet()
    {
        var result = PagingHelper.Normalize(requestedPage: 0, pageSize: 48, totalCount: 0);

        Assert.Equal(1, result.Page);
        Assert.Equal(48, result.PageSize);
        Assert.Equal(1, result.TotalPages);
        Assert.Equal(0, result.Skip);
    }
}
