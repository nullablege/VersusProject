using Kiyaslasana.BL.Abstractions;
using Kiyaslasana.DAL.Data;
using Kiyaslasana.EL.Entities;
using Microsoft.EntityFrameworkCore;

namespace Kiyaslasana.DAL.Repositories;

public sealed class EfTelefonRepository : ITelefonRepository
{
    private readonly KiyaslasanaDbContext _dbContext;

    public EfTelefonRepository(KiyaslasanaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Telefon?> GetBySlugAsync(string slug, CancellationToken ct)
    {
        return await _dbContext.Telefonlar
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Slug == slug, ct);
    }

    public async Task<IReadOnlyList<Telefon>> GetBySlugsAsync(IReadOnlyList<string> slugs, CancellationToken ct)
    {
        if (slugs.Count == 0)
        {
            return [];
        }

        var normalized = slugs
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim().ToLowerInvariant())
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (normalized.Length == 0)
        {
            return [];
        }

        var items = await _dbContext.Telefonlar
            .AsNoTracking()
            .Where(x => x.Slug != null && normalized.Contains(x.Slug))
            .ToListAsync(ct);

        var bySlug = items
            .Where(x => !string.IsNullOrWhiteSpace(x.Slug))
            .ToDictionary(x => x.Slug!, StringComparer.Ordinal);

        var ordered = new List<Telefon>(normalized.Length);
        foreach (var slug in normalized)
        {
            if (bySlug.TryGetValue(slug, out var telefon))
            {
                ordered.Add(telefon);
            }
        }

        return ordered;
    }

    public async Task<IReadOnlyList<Telefon>> GetLatestAsync(int take, CancellationToken ct)
    {
        var safeTake = Math.Clamp(take, 1, 50);

        return await _dbContext.Telefonlar
            .AsNoTracking()
            .OrderByDescending(x => x.KayitTarihi)
            .ThenByDescending(x => x.Id)
            .Take(safeTake)
            .ToListAsync(ct);
    }

    public Task<int> CountAsync(CancellationToken ct)
    {
        return _dbContext.Telefonlar
            .AsNoTracking()
            .CountAsync(x => !string.IsNullOrWhiteSpace(x.Slug), ct);
    }

    public async Task<IReadOnlyList<string>> GetSlugsPageAsync(int skip, int take, CancellationToken ct)
    {
        if (take <= 0)
        {
            return [];
        }

        var safeSkip = Math.Max(skip, 0);

        return await _dbContext.Telefonlar
            .AsNoTracking()
            .Where(x => !string.IsNullOrWhiteSpace(x.Slug))
            .Select(x => x.Slug!)
            .OrderBy(x => x)
            .Skip(safeSkip)
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task<(IReadOnlyList<Telefon> Items, int TotalCount)> GetPagedAsync(int skip, int take, CancellationToken ct)
    {
        if (take <= 0)
        {
            return ([], 0);
        }

        var safeSkip = Math.Max(skip, 0);
        var baseQuery = _dbContext.Telefonlar
            .AsNoTracking()
            .Where(x => !string.IsNullOrWhiteSpace(x.Slug));

        var totalCount = await baseQuery.CountAsync(ct);
        var items = await baseQuery
            .OrderBy(x => x.Slug)
            .Skip(safeSkip)
            .Take(take)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<(IReadOnlyList<Telefon> Items, int TotalCount)> GetPagedByBrandAsync(string brand, int skip, int take, CancellationToken ct)
    {
        if (take <= 0 || string.IsNullOrWhiteSpace(brand))
        {
            return ([], 0);
        }

        var safeSkip = Math.Max(skip, 0);
        var baseQuery = _dbContext.Telefonlar
            .AsNoTracking()
            .Where(x => !string.IsNullOrWhiteSpace(x.Slug) && x.Marka == brand);

        var totalCount = await baseQuery.CountAsync(ct);
        var items = await baseQuery
            .OrderBy(x => x.Slug)
            .Skip(safeSkip)
            .Take(take)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<IReadOnlyList<string>> GetDistinctBrandsAsync(CancellationToken ct)
    {
        return await _dbContext.Telefonlar
            .AsNoTracking()
            .Where(x => !string.IsNullOrWhiteSpace(x.Marka))
            .Select(x => x.Marka!.Trim())
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(ct);
    }
}
