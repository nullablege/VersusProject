using System.Linq.Expressions;
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

    public async Task<List<Telefon>> GetBySlugsAsync(IEnumerable<string> slugs, CancellationToken ct)
    {
        var normalized = slugs
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim().ToLowerInvariant())
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (normalized.Length == 0)
        {
            return [];
        }

        return await _dbContext.Telefonlar
            .AsNoTracking()
            .Where(x => !string.IsNullOrWhiteSpace(x.Slug) && normalized.Contains(x.Slug!))
            .ToListAsync(ct);
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

    public async Task<IReadOnlyList<Telefon>> GetRelatedCandidatesAsync(int take, CancellationToken ct)
    {
        var safeTake = Math.Clamp(take, 1, 500);

        return await _dbContext.Telefonlar
            .AsNoTracking()
            .Where(x => !string.IsNullOrWhiteSpace(x.Slug))
            .OrderByDescending(x => x.KayitTarihi)
            .ThenByDescending(x => x.Id)
            .Select(x => new Telefon
            {
                Slug = x.Slug,
                ModelAdi = x.ModelAdi,
                Marka = x.Marka,
                ResimUrl = x.ResimUrl
            })
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

    public async Task<(IReadOnlyList<Telefon> Items, int TotalCount)> GetPagedByPredicateAsync(
        Expression<Func<Telefon, bool>> predicate,
        int skip,
        int take,
        CancellationToken ct)
    {
        if (take <= 0)
        {
            return ([], 0);
        }

        var safeSkip = Math.Max(skip, 0);
        var baseQuery = _dbContext.Telefonlar
            .AsNoTracking()
            .Where(x => !string.IsNullOrWhiteSpace(x.Slug))
            .Where(predicate);

        var totalCount = await baseQuery.CountAsync(ct);
        var maxSkip = totalCount <= 0 ? 0 : ((totalCount - 1) / take) * take;
        var effectiveSkip = Math.Min(safeSkip, maxSkip);

        var items = await baseQuery
            .OrderBy(x => x.Slug)
            .Skip(effectiveSkip)
            .Take(take)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<IReadOnlyList<Telefon>> GetLatestByBrandAsync(string brand, int take, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(brand) || take <= 0)
        {
            return [];
        }

        var safeTake = Math.Clamp(take, 1, 50);

        return await _dbContext.Telefonlar
            .AsNoTracking()
            .Where(x => !string.IsNullOrWhiteSpace(x.Slug) && x.Marka == brand)
            .OrderByDescending(x => x.KayitTarihi)
            .ThenByDescending(x => x.Id)
            .Take(safeTake)
            .ToListAsync(ct);
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
