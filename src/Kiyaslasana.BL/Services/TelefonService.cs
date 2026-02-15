using System.Text.RegularExpressions;
using Kiyaslasana.BL.Abstractions;
using Kiyaslasana.BL.Contracts;
using Kiyaslasana.EL.Entities;
using Microsoft.Extensions.Caching.Memory;

namespace Kiyaslasana.BL.Services;

public sealed class TelefonService : ITelefonService
{
    private static readonly Regex CacheableSlugRegex = new(
        "^[a-z0-9-]{1,120}$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

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

    public async Task<CompareResolveResult> ResolveCompareAsync(IEnumerable<string> slugs, bool isAuthenticated, CancellationToken ct)
    {
        var maxAllowed = isAuthenticated ? 4 : 2;
        var normalized = NormalizeDistinctSlugs(slugs);

        if (normalized.Count < 2)
        {
            return new CompareResolveResult(false, "Karsilastirma icin en az 2 telefon gerekli.", [], [], maxAllowed);
        }

        if (normalized.Count > maxAllowed)
        {
            return new CompareResolveResult(false, $"Karsilastirma limiti asildi. En fazla {maxAllowed} telefon secilebilir.", [], [], maxAllowed);
        }

        var canonical = normalized
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToArray();

        var items = await _telefonRepository.GetBySlugsAsync(canonical, ct);
        var bySlug = items
            .Where(x => !string.IsNullOrWhiteSpace(x.Slug))
            .ToDictionary(x => x.Slug!, StringComparer.Ordinal);

        var orderedPhones = new List<Telefon>(canonical.Length);
        foreach (var slug in canonical)
        {
            if (!bySlug.TryGetValue(slug, out var phone))
            {
                return new CompareResolveResult(false, "Secilen sluglardan biri bulunamadi.", [], [], maxAllowed);
            }

            orderedPhones.Add(phone);
        }

        return new CompareResolveResult(true, null, canonical, orderedPhones, maxAllowed);
    }

    public async Task<Telefon?> GetBySlugAsync(string slug, CancellationToken ct)
    {
        var normalizedSlug = NormalizeSlug(slug);
        if (!IsCacheableSlug(normalizedSlug))
        {
            // Reject invalid/high-cardinality keys early to avoid cache abuse.
            return null;
        }

        var cacheKey = $"telefon:slug:{normalizedSlug}";
        if (_memoryCache.TryGetValue<Telefon>(cacheKey, out var cachedPhone))
        {
            return cachedPhone;
        }

        var phone = await _telefonRepository.GetBySlugAsync(normalizedSlug, ct);
        if (phone is null)
        {
            // Do not cache misses to avoid unbounded negative-cache entries.
            return null;
        }

        _memoryCache.Set(cacheKey, phone, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = PhoneCacheDuration,
            Size = 1
        });

        return phone;
    }

    public async Task<IReadOnlyList<Telefon>> GetBySlugsAsync(IReadOnlyList<string> slugs, CancellationToken ct)
    {
        var normalized = NormalizeDistinctSlugs(slugs);
        if (normalized.Count == 0)
        {
            return [];
        }

        var items = await _telefonRepository.GetBySlugsAsync(normalized, ct);
        var bySlug = items
            .Where(x => !string.IsNullOrWhiteSpace(x.Slug))
            .ToDictionary(x => x.Slug!, StringComparer.Ordinal);

        var ordered = new List<Telefon>(normalized.Count);
        foreach (var slug in normalized)
        {
            if (bySlug.TryGetValue(slug, out var phone))
            {
                ordered.Add(phone);
            }
        }

        return ordered;
    }

    public async Task<IReadOnlyList<Telefon>> GetLatestAsync(int take, CancellationToken ct)
    {
        var safeTake = Math.Clamp(take, 1, 50);
        var cacheKey = $"telefon:latest:{safeTake}";

        if (_memoryCache.TryGetValue<IReadOnlyList<Telefon>>(cacheKey, out var cachedList) && cachedList is not null)
        {
            return cachedList;
        }

        var latest = await _telefonRepository.GetLatestAsync(safeTake, ct);
        var value = latest ?? [];

        _memoryCache.Set(cacheKey, value, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ListCacheDuration,
            Size = 1
        });

        return value;
    }

    private List<string> NormalizeDistinctSlugs(IEnumerable<string> slugs)
    {
        var result = new List<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var slug in slugs)
        {
            var normalized = NormalizeSlug(slug);
            if (normalized.Length == 0)
            {
                continue;
            }

            if (seen.Add(normalized))
            {
                result.Add(normalized);
            }
        }

        return result;
    }

    private static bool IsCacheableSlug(string slug)
    {
        return CacheableSlugRegex.IsMatch(slug);
    }
}
