using Kiyaslasana.BL.Contracts;

namespace Kiyaslasana.PL.Areas.Admin.ViewModels;

public sealed class TelefonReviewAdminListViewModel
{
    public required IReadOnlyList<TelefonReviewAdminListItem> Items { get; init; }

    public required int Page { get; init; }

    public required int PageSize { get; init; }

    public required int TotalCount { get; init; }

    public required int TotalPages { get; init; }

    public string? Query { get; init; }
}
