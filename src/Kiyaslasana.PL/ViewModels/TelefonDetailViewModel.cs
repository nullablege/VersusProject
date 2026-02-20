using Kiyaslasana.EL.Entities;

namespace Kiyaslasana.PL.ViewModels;

public sealed class TelefonDetailViewModel
{
    public required Telefon Telefon { get; init; }
    public TelefonReview? Review { get; init; }
    public required string ProductJsonLd { get; init; }
    public required string BreadcrumbJsonLd { get; init; }
    public required IReadOnlyList<CompareRelatedLinkViewModel> CompareSuggestions { get; init; }
    public required IReadOnlyList<Telefon> SimilarPhones { get; init; }
    public required IReadOnlyList<CompareRelatedLinkViewModel> TopComparedLinks { get; init; }
    public required IReadOnlyList<BlogPost> RelatedBlogPosts { get; init; }
}
