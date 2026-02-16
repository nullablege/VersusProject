using Kiyaslasana.EL.Entities;

namespace Kiyaslasana.PL.ViewModels;

public sealed class BlogListViewModel
{
    public required IReadOnlyList<BlogPost> Items { get; init; }

    public required int Page { get; init; }

    public required int PageSize { get; init; }

    public required int TotalCount { get; init; }

    public required int TotalPages { get; init; }

    public required string CanonicalUrl { get; init; }

    public string? PrevUrl { get; init; }

    public string? NextUrl { get; init; }

    public required string RobotsMeta { get; init; }
}
