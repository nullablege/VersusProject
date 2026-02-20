using Kiyaslasana.BL.Contracts;

namespace Kiyaslasana.BL.Abstractions;

public interface ICompareVisitRepository
{
    Task<bool> TryAddVisitAsync(
        string canonicalLeftSlug,
        string canonicalRightSlug,
        DateTimeOffset visitedAt,
        string? ipHash,
        TimeSpan dedupeWindow,
        CancellationToken ct);

    Task<IReadOnlyList<TopComparedPair>> GetTopComparedAsync(int take, CancellationToken ct);

    Task<IReadOnlyList<TopComparedPair>> GetTopComparedBySlugAsync(string slug, int take, CancellationToken ct);
}
