using Kiyaslasana.BL.Abstractions;
using Kiyaslasana.BL.Contracts;
using Kiyaslasana.DAL.Data;
using Kiyaslasana.EL.Entities;
using Microsoft.EntityFrameworkCore;

namespace Kiyaslasana.DAL.Repositories;

public sealed class EfCompareVisitRepository : ICompareVisitRepository
{
    private readonly KiyaslasanaDbContext _dbContext;

    public EfCompareVisitRepository(KiyaslasanaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> TryAddVisitAsync(
        string canonicalLeftSlug,
        string canonicalRightSlug,
        DateTimeOffset visitedAt,
        string? ipHash,
        TimeSpan dedupeWindow,
        CancellationToken ct)
    {
        var windowStart = visitedAt.Subtract(dedupeWindow);
        var hasRecent = await _dbContext.CompareVisits
            .AsNoTracking()
            .AnyAsync(x =>
                x.CanonicalLeftSlug == canonicalLeftSlug &&
                x.CanonicalRightSlug == canonicalRightSlug &&
                x.IpHash == ipHash &&
                x.VisitedAt >= windowStart, ct);
        if (hasRecent)
        {
            return false;
        }

        _dbContext.CompareVisits.Add(new CompareVisit
        {
            CanonicalLeftSlug = canonicalLeftSlug,
            CanonicalRightSlug = canonicalRightSlug,
            VisitedAt = visitedAt,
            IpHash = ipHash
        });

        await _dbContext.SaveChangesAsync(ct);
        return true;
    }

    public async Task<IReadOnlyList<TopComparedPair>> GetTopComparedAsync(int take, CancellationToken ct)
    {
        var safeTake = Math.Clamp(take, 1, 100);

        return await _dbContext.CompareVisits
            .AsNoTracking()
            .GroupBy(x => new { x.CanonicalLeftSlug, x.CanonicalRightSlug })
            .Select(x => new TopComparedPair(
                x.Key.CanonicalLeftSlug,
                x.Key.CanonicalRightSlug,
                x.Count()))
            .OrderByDescending(x => x.VisitCount)
            .ThenBy(x => x.CanonicalLeftSlug)
            .ThenBy(x => x.CanonicalRightSlug)
            .Take(safeTake)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<TopComparedPair>> GetTopComparedBySlugAsync(string slug, int take, CancellationToken ct)
    {
        var safeTake = Math.Clamp(take, 1, 100);

        return await _dbContext.CompareVisits
            .AsNoTracking()
            .Where(x => x.CanonicalLeftSlug == slug || x.CanonicalRightSlug == slug)
            .GroupBy(x => new { x.CanonicalLeftSlug, x.CanonicalRightSlug })
            .Select(x => new TopComparedPair(
                x.Key.CanonicalLeftSlug,
                x.Key.CanonicalRightSlug,
                x.Count()))
            .OrderByDescending(x => x.VisitCount)
            .ThenBy(x => x.CanonicalLeftSlug)
            .ThenBy(x => x.CanonicalRightSlug)
            .Take(safeTake)
            .ToListAsync(ct);
    }
}
