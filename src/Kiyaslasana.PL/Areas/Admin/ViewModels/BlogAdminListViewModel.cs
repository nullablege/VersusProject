using Kiyaslasana.EL.Entities;

namespace Kiyaslasana.PL.Areas.Admin.ViewModels;

public sealed class BlogAdminListViewModel
{
    public required IReadOnlyList<BlogPost> Items { get; init; }

    public required int Page { get; init; }

    public required int PageSize { get; init; }

    public required int TotalCount { get; init; }

    public required int TotalPages { get; init; }
}
