using Kiyaslasana.EL.Entities;

namespace Kiyaslasana.PL.ViewModels;

public sealed class CompareViewModel
{
    public required IReadOnlyList<Telefon> Phones { get; init; }

    public required bool IsSeoIndexable { get; init; }

    public required string CanonicalUrl { get; init; }

    public required string PageTitle { get; init; }

    public required string MetaDescription { get; init; }

    public string? BreadcrumbJsonLd { get; init; }
}
