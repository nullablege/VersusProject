namespace Kiyaslasana.BL.Helpers;

public static class PagingHelper
{
    public static (int Page, int PageSize, int TotalPages, int Skip) Normalize(int requestedPage, int pageSize, int totalCount)
    {
        var safePageSize = Math.Max(pageSize, 1);
        var safeTotalCount = Math.Max(totalCount, 0);
        var totalPages = Math.Max(1, (int)Math.Ceiling(safeTotalCount / (double)safePageSize));
        var page = Math.Clamp(requestedPage, 1, totalPages);
        var skip = (page - 1) * safePageSize;

        return (page, safePageSize, totalPages, skip);
    }
}
