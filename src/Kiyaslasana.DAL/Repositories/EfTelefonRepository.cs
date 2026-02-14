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
            .CountAsync(x => x.Slug != null && x.Slug != string.Empty, ct);
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
            .Where(x => x.Slug != null && x.Slug != string.Empty)
            .Select(x => x.Slug!)
            .OrderBy(x => EF.Functions.Collate(x, "Latin1_General_100_BIN2"))
            .Skip(safeSkip)
            .Take(take)
            .ToListAsync(ct);
    }
}
