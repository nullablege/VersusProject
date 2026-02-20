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

    public async Task AddVisitAsync(string slugLeft, string slugRight, DateTimeOffset visitedAt, string? ipHash, CancellationToken ct)
    {
        _dbContext.CompareVisits.Add(new CompareVisit
        {
            SlugLeft = slugLeft,
            SlugRight = slugRight,
            VisitedAt = visitedAt,
            IPHash = ipHash
        });

        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<TopComparedPair>> GetTopComparedAsync(int take, CancellationToken ct)
    {
        var safeTake = Math.Clamp(take, 1, 100);

        return await _dbContext.CompareVisits
            .AsNoTracking()
            .GroupBy(x => new { x.SlugLeft, x.SlugRight })
            .Select(x => new TopComparedPair(
                x.Key.SlugLeft,
                x.Key.SlugRight,
                x.Count()))
            .OrderByDescending(x => x.VisitCount)
            .ThenBy(x => x.SlugLeft)
            .ThenBy(x => x.SlugRight)
            .Take(safeTake)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<TopComparedPair>> GetTopComparedBySlugAsync(string slug, int take, CancellationToken ct)
    {
        var safeTake = Math.Clamp(take, 1, 100);

        return await _dbContext.CompareVisits
            .AsNoTracking()
            .Where(x => x.SlugLeft == slug || x.SlugRight == slug)
            .GroupBy(x => new { x.SlugLeft, x.SlugRight })
            .Select(x => new TopComparedPair(
                x.Key.SlugLeft,
                x.Key.SlugRight,
                x.Count()))
            .OrderByDescending(x => x.VisitCount)
            .ThenBy(x => x.SlugLeft)
            .ThenBy(x => x.SlugRight)
            .Take(safeTake)
            .ToListAsync(ct);
    }
}
