using Kiyaslasana.EL.Entities;

namespace Kiyaslasana.PL.ViewModels;

public sealed class TelefonListViewModel
{
    public required IReadOnlyList<Telefon> Items { get; init; }

    public required IReadOnlyList<BrandLinkViewModel> Brands { get; init; }

    public required int Page { get; init; }

    public required int PageSize { get; init; }

    public required int TotalCount { get; init; }

    public required int TotalPages { get; init; }

    public string? Brand { get; init; }

    public string? BrandSlug { get; init; }

    public required string BasePath { get; init; }

    public required string CanonicalUrl { get; init; }

    public string? PrevUrl { get; init; }

    public string? NextUrl { get; init; }

    public required string RobotsMeta { get; init; }
}

public sealed class BrandLinkViewModel
{
    public required string Name { get; init; }

    public required string Slug { get; init; }

    public required string Url { get; init; }

    public required bool IsActive { get; init; }
}
