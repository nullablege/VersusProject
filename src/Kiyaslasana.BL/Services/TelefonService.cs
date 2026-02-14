using Kiyaslasana.BL.Abstractions;
using Kiyaslasana.BL.Contracts;
using Kiyaslasana.EL.Entities;
using Microsoft.Extensions.Caching.Memory;

namespace Kiyaslasana.BL.Services;

public sealed class TelefonService : ITelefonService
{
    private static readonly TimeSpan PhoneCacheDuration = TimeSpan.FromHours(6);
    private static readonly TimeSpan ListCacheDuration = TimeSpan.FromMinutes(15);

    private readonly ITelefonRepository _telefonRepository;
    private readonly IMemoryCache _memoryCache;

    public TelefonService(ITelefonRepository telefonRepository, IMemoryCache memoryCache)
    {
        _telefonRepository = telefonRepository;
        _memoryCache = memoryCache;
    }

    public string NormalizeSlug(string? slug)
    {
        return (slug ?? string.Empty).Trim().ToLowerInvariant();
    }

    public CompareParseResult ParseCompareSlugs(string? slugs, bool isAuthenticated)
    {
        var maxAllowed = isAuthenticated ? 4 : 2;

        if (string.IsNullOrWhiteSpace(slugs))
        {
            return new CompareParseResult(false, "Karsilastirma icin slug bilgisi gerekli.", [], [], maxAllowed);
        }

        var requested = new List<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var part in slugs.Split("-vs-", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var normalized = NormalizeSlug(part);
            if (normalized.Length == 0)
            {
                continue;
            }

            if (seen.Add(normalized))
            {
                requested.Add(normalized);
            }
        }

        if (requested.Count < 2)
        {
            return new CompareParseResult(false, "Karsilastirma icin en az 2 telefon gerekli.", requested, [], maxAllowed);
        }

        if (requested.Count > maxAllowed)
        {
            return new CompareParseResult(false, $"Karsilastirma limiti asildi. En fazla {maxAllowed} telefon secilebilir.", requested, [], maxAllowed);
        }

        var canonical = requested.OrderBy(x => x, StringComparer.Ordinal).ToArray();
        return new CompareParseResult(true, null, requested, canonical, maxAllowed);
    }

    public async Task<Telefon?> GetBySlugAsync(string slug, CancellationToken ct)
    {
        var normalizedSlug = NormalizeSlug(slug);
        if (normalizedSlug.Length == 0)
        {
            return null;
        }

        var cacheKey = $"telefon:slug:{normalizedSlug}";
        return await _memoryCache.GetOrCreateAsync(cacheKey, async cacheEntry =>
        {
            cacheEntry.AbsoluteExpirationRelativeToNow = PhoneCacheDuration;
            return await _telefonRepository.GetBySlugAsync(normalizedSlug, ct);
        });
    }

    public async Task<IReadOnlyList<Telefon>> GetBySlugsAsync(IReadOnlyList<string> slugs, CancellationToken ct)
    {
        if (slugs.Count == 0)
        {
            return [];
        }

        var normalized = slugs
            .Select(NormalizeSlug)
            .Where(x => x.Length > 0)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (normalized.Length == 0)
        {
            return [];
        }

        var list = new List<Telefon>(normalized.Length);
        foreach (var slug in normalized)
        {
            var item = await GetBySlugAsync(slug, ct);
            if (item is not null)
            {
                list.Add(item);
            }
        }

        return list;
    }

    public async Task<IReadOnlyList<Telefon>> GetLatestAsync(int take, CancellationToken ct)
    {
        var safeTake = Math.Clamp(take, 1, 50);
        var cacheKey = $"telefon:latest:{safeTake}";

        return await _memoryCache.GetOrCreateAsync(cacheKey, async cacheEntry =>
        {
            cacheEntry.AbsoluteExpirationRelativeToNow = ListCacheDuration;
            return await _telefonRepository.GetLatestAsync(safeTake, ct);
        }) ?? [];
    }
}
