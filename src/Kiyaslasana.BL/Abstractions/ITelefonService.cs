using Kiyaslasana.BL.Contracts;
using Kiyaslasana.EL.Entities;

namespace Kiyaslasana.BL.Abstractions;

public interface ITelefonService
{
    string NormalizeSlug(string? slug);

    CompareParseResult ParseCompareSlugs(string? slugs, bool isAuthenticated);

    Task<CompareResolveResult> ResolveCompareAsync(IEnumerable<string> slugs, bool isAuthenticated, CancellationToken ct);

    Task<Telefon?> GetBySlugAsync(string slug, CancellationToken ct);

    Task<IReadOnlyList<Telefon>> GetBySlugsAsync(IReadOnlyList<string> slugs, CancellationToken ct);

    Task<IReadOnlyList<Telefon>> GetLatestAsync(int take, CancellationToken ct);

    Task<IReadOnlyList<RelatedComparisonLink>> GetRelatedComparisonLinksAsync(
        IReadOnlyList<string> currentSlugs,
        int perSlug,
        int totalMax,
        CancellationToken ct);
}
