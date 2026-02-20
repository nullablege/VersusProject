using Kiyaslasana.BL.Contracts;

namespace Kiyaslasana.BL.Abstractions;

public interface ICompareVisitRepository
{
    Task AddVisitAsync(string slugLeft, string slugRight, DateTimeOffset visitedAt, string? ipHash, CancellationToken ct);

    Task<IReadOnlyList<TopComparedPair>> GetTopComparedAsync(int take, CancellationToken ct);

    Task<IReadOnlyList<TopComparedPair>> GetTopComparedBySlugAsync(string slug, int take, CancellationToken ct);
}
