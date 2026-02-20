using System.Text.Json;
using System.Text.RegularExpressions;
using System.Net;
using Kiyaslasana.BL.Abstractions;
using Kiyaslasana.BL.Contracts;
using Kiyaslasana.BL.Helpers;
using Kiyaslasana.BL.SeoFilters;
using Kiyaslasana.EL.Entities;
using Kiyaslasana.PL.Infrastructure;
using Kiyaslasana.PL.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Caching.Memory;

namespace Kiyaslasana.PL.Controllers;

public sealed class TelefonController : SeoControllerBase
{
    private const int ListingPageSize = 48;
    private const string BrandSlugMapCacheKey = "telefon:list:brand-slug-map:v1";
    private const string BrandPopularCompareCacheKeyPrefix = "telefon:list:brand-popular-compare:v1";
    private const string DetailCompareCacheKeyPrefix = "telefon:detail:compare-links:v1";
    private static readonly TimeSpan BrandSlugMapCacheDuration = TimeSpan.FromHours(24);
    private static readonly TimeSpan BrandPopularCompareCacheDuration = TimeSpan.FromMinutes(20);
    private static readonly TimeSpan DetailCompareCacheDuration = TimeSpan.FromHours(1);
    private static readonly Regex HtmlTagRegex = new("<.*?>", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant);
    private static readonly Regex WsRegex = new("\\s+", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private readonly ITelefonService _telefonService;
    private readonly ITelefonReviewService _telefonReviewService;
    private readonly IBlogPostService _blogPostService;
    private readonly ITelefonRepository _telefonRepository;
    private readonly IMemoryCache _memoryCache;

    public TelefonController(
        ITelefonService telefonService,
        ITelefonReviewService telefonReviewService,
        IBlogPostService blogPostService,
        ITelefonRepository telefonRepository,
        IMemoryCache memoryCache)
    {
        _telefonService = telefonService;
        _telefonReviewService = telefonReviewService;
        _blogPostService = blogPostService;
        _telefonRepository = telefonRepository;
        _memoryCache = memoryCache;
    }

    [HttpGet("/telefonlar")]
    [OutputCache(PolicyName = OutputCachePolicyNames.AnonymousOneHour, VaryByQueryKeys = ["page"])]
    public async Task<IActionResult> Index([FromQuery] int page = 1, CancellationToken ct = default)
    {
        var requestedPage = Math.Max(page, 1);
        var initialSkip = (requestedPage - 1) * ListingPageSize;
        const string title = "Tum Telefonlar";
        const string description = "Veritabanina eklenmis tum telefon modelleri.";

        var (items, totalCount) = await _telefonRepository.GetPagedAsync(initialSkip, ListingPageSize, ct);
        var paging = PagingHelper.Normalize(requestedPage, ListingPageSize, totalCount);

        if (paging.Skip != initialSkip)
        {
            var clampedResult = await _telefonRepository.GetPagedAsync(paging.Skip, ListingPageSize, ct);
            items = clampedResult.Items;
            totalCount = clampedResult.TotalCount;
        }

        var brandMap = await GetBrandSlugMapAsync(ct);
        var viewModel = BuildListViewModel(
            items: items,
            totalCount: totalCount,
            page: paging.Page,
            totalPages: paging.TotalPages,
            pageSize: paging.PageSize,
            brandMap: brandMap,
            basePath: "/telefonlar",
            selectedBrandSlug: null,
            selectedBrand: null,
            popularComparisons: [],
            filterPopularComparison: null,
            listingTitle: title,
            listingDescription: description,
            useFirstPageCanonicalForPagedResults: false,
            noindexPagedResults: false);

        ApplyListingSeo(viewModel, title, description);

        ViewData["Nav"] = "telefonlar";
        return View(viewModel);
    }

    [HttpGet("/telefonlar/marka/{brandSlug}")]
    [OutputCache(
        PolicyName = OutputCachePolicyNames.AnonymousOneHour,
        VaryByRouteValueNames = ["brandSlug"],
        VaryByQueryKeys = ["page"])]
    public async Task<IActionResult> ByBrand(string brandSlug, [FromQuery] int page = 1, CancellationToken ct = default)
    {
        var normalizedBrandSlug = BrandSlugHelper.ToSlug(brandSlug);
        if (normalizedBrandSlug.Length == 0)
        {
            return NotFound();
        }

        var brandMap = await GetBrandSlugMapAsync(ct);
        if (!brandMap.TryGetValue(normalizedBrandSlug, out var brand))
        {
            return NotFound();
        }

        var requestedPage = Math.Max(page, 1);
        var initialSkip = (requestedPage - 1) * ListingPageSize;

        var (items, totalCount) = await _telefonRepository.GetPagedByBrandAsync(brand, initialSkip, ListingPageSize, ct);
        var paging = PagingHelper.Normalize(requestedPage, ListingPageSize, totalCount);

        if (paging.Skip != initialSkip)
        {
            var clampedResult = await _telefonRepository.GetPagedByBrandAsync(brand, paging.Skip, ListingPageSize, ct);
            items = clampedResult.Items;
            totalCount = clampedResult.TotalCount;
        }

        var basePath = $"/telefonlar/marka/{normalizedBrandSlug}";
        var popularComparisons = await GetBrandPopularComparisonsAsync(brand, normalizedBrandSlug, ct);
        var listingTitle = $"{brand} Telefonlari";
        var listingDescription = $"{brand} markasina ait telefon modelleri.";
        var viewModel = BuildListViewModel(
            items: items,
            totalCount: totalCount,
            page: paging.Page,
            totalPages: paging.TotalPages,
            pageSize: paging.PageSize,
            brandMap: brandMap,
            basePath: basePath,
            selectedBrandSlug: normalizedBrandSlug,
            selectedBrand: brand,
            popularComparisons: popularComparisons,
            filterPopularComparison: null,
            listingTitle: listingTitle,
            listingDescription: listingDescription,
            useFirstPageCanonicalForPagedResults: true,
            noindexPagedResults: true);

        ApplyListingSeo(viewModel, listingTitle, listingDescription);

        ViewData["Nav"] = "telefonlar";
        return View("Index", viewModel);
    }

    [HttpGet("/telefonlar/{filterSlug}")]
    [OutputCache(
        PolicyName = OutputCachePolicyNames.AnonymousOneHour,
        VaryByRouteValueNames = ["filterSlug"],
        VaryByQueryKeys = ["page"])]
    public async Task<IActionResult> ByFilter(string filterSlug, [FromQuery] int page = 1, CancellationToken ct = default)
    {
        if (page < 1)
        {
            return NotFound();
        }

        if (!SeoFilterRegistry.TryGet(filterSlug, out var filter))
        {
            return NotFound();
        }

        var initialSkip = (page - 1) * ListingPageSize;
        var (items, totalCount) = await _telefonRepository.GetPagedByPredicateAsync(filter.Predicate, initialSkip, ListingPageSize, ct);
        var paging = PagingHelper.Normalize(page, ListingPageSize, totalCount);

        var brandMap = await GetBrandSlugMapAsync(ct);
        var basePath = $"/telefonlar/{filter.Slug}";
        var filterPopularComparison = BuildFilterPopularComparison(items);
        var viewModel = BuildListViewModel(
            items: items,
            totalCount: totalCount,
            page: paging.Page,
            totalPages: paging.TotalPages,
            pageSize: paging.PageSize,
            brandMap: brandMap,
            basePath: basePath,
            selectedBrandSlug: null,
            selectedBrand: null,
            popularComparisons: [],
            filterPopularComparison: filterPopularComparison,
            listingTitle: filter.Title,
            listingDescription: filter.MetaDescription,
            useFirstPageCanonicalForPagedResults: true,
            noindexPagedResults: true);

        ApplyListingSeo(viewModel, filter.Title, filter.MetaDescription);
        ViewData["Nav"] = "telefonlar";
        ViewData["FilterItemListJsonLd"] = BuildFilterItemListJsonLd(items);

        return View("Index", viewModel);
    }

    [HttpGet("/telefon/{slug:telefonslug}")]
    [OutputCache(PolicyName = OutputCachePolicyNames.AnonymousOneDay, VaryByRouteValueNames = ["slug"])]
    public async Task<IActionResult> Detail(string slug, CancellationToken ct)
    {
        var normalizedSlug = _telefonService.NormalizeSlug(slug);
        var phone = await _telefonService.GetBySlugAsync(normalizedSlug, ct);

        if (phone is null)
        {
            return NotFound();
        }

        var title = BuildPhoneTitle(phone);
        var review = await _telefonReviewService.GetByTelefonSlugAsync(normalizedSlug, ct);
        var seoTitle = !string.IsNullOrWhiteSpace(review?.SeoTitle) ? review.SeoTitle : title;
        var seoDescription = !string.IsNullOrWhiteSpace(review?.SeoDescription)
            ? review.SeoDescription
            : $"{title} teknik ozellikleri, fiyat bilgisi ve karsilastirma detaylari.";
        var reviewBody = BuildReviewBodyForSchema(review?.SanitizedContent);
        var schemaDescription = !string.IsNullOrWhiteSpace(review?.SeoDescription)
            ? review.SeoDescription
            : !string.IsNullOrWhiteSpace(review?.Excerpt)
                ? review.Excerpt
                : !string.IsNullOrWhiteSpace(reviewBody)
                    ? reviewBody
                    : seoDescription;

        SetSeo(
            title: seoTitle!,
            description: seoDescription!,
            canonicalUrl: BuildAbsoluteUrl($"/telefon/{normalizedSlug}"));

        ViewData["Nav"] = "telefonlar";
        var compareSuggestions = await GetDetailCompareSuggestionsAsync(normalizedSlug, title, ct);
        var similarPhones = await _telefonService.GetSimilarPhonesAsync(normalizedSlug, 4, ct);
        var topComparedLinks = await GetDetailTopComparedLinksAsync(normalizedSlug, title, ct);
        var relatedBlogPosts = await _blogPostService.GetLatestPublishedMentioningTelefonSlugAsync(normalizedSlug, 3, ct);

        return View(new TelefonDetailViewModel
        {
            Telefon = phone,
            Review = review,
            ProductJsonLd = BuildProductJsonLd(phone, normalizedSlug, schemaDescription, review, reviewBody),
            BreadcrumbJsonLd = BuildBreadcrumbJsonLd(phone, normalizedSlug),
            CompareSuggestions = compareSuggestions,
            SimilarPhones = similarPhones,
            TopComparedLinks = topComparedLinks,
            RelatedBlogPosts = relatedBlogPosts
        });
    }

    private static string BuildPhoneTitle(Telefon phone)
    {
        return string.Join(' ', new[] { phone.Marka, phone.ModelAdi }.Where(x => !string.IsNullOrWhiteSpace(x))).Trim();
    }

    private string BuildProductJsonLd(
        Telefon phone,
        string slug,
        string? description,
        TelefonReview? review,
        string? reviewBody)
    {
        var modelName = string.IsNullOrWhiteSpace(phone.ModelAdi) ? BuildPhoneTitle(phone) : phone.ModelAdi;
        var data = new Dictionary<string, object?>
        {
            ["@context"] = "https://schema.org",
            ["@type"] = "Product",
            ["name"] = modelName,
            ["brand"] = new Dictionary<string, object?>
            {
                ["@type"] = "Brand",
                ["name"] = phone.Marka
            },
            ["url"] = BuildAbsoluteUrl($"/telefon/{slug}"),
            ["description"] = description,
            ["sku"] = slug
        };

        if (!string.IsNullOrWhiteSpace(reviewBody) && review is not null && review.CreatedAt != default)
        {
            data["review"] = new Dictionary<string, object?>
            {
                ["@type"] = "Review",
                ["reviewBody"] = reviewBody,
                ["datePublished"] = review.CreatedAt.UtcDateTime.ToString("O")
            };
        }

        return JsonSerializer.Serialize(data);
    }

    private static string? BuildReviewBodyForSchema(string? sanitizedContent)
    {
        if (string.IsNullOrWhiteSpace(sanitizedContent))
        {
            return null;
        }

        var withoutHtml = HtmlTagRegex.Replace(sanitizedContent, " ");
        var decoded = WebUtility.HtmlDecode(withoutHtml);
        var compact = WsRegex.Replace(decoded, " ").Trim();

        if (compact.Length == 0)
        {
            return null;
        }

        return compact.Length <= 300 ? compact : compact[..300];
    }

    private string BuildBreadcrumbJsonLd(Telefon phone, string slug)
    {
        var title = BuildPhoneTitle(phone);

        var data = new
        {
            @context = "https://schema.org",
            @type = "BreadcrumbList",
            itemListElement = new object[]
            {
                new
                {
                    @type = "ListItem",
                    position = 1,
                    name = "Ana Sayfa",
                    item = BuildAbsoluteUrl("/")
                },
                new
                {
                    @type = "ListItem",
                    position = 2,
                    name = "Telefonlar",
                    item = BuildAbsoluteUrl("/telefonlar")
                },
                new
                {
                    @type = "ListItem",
                    position = 3,
                    name = title,
                    item = BuildAbsoluteUrl($"/telefon/{slug}")
                }
            }
        };

        return JsonSerializer.Serialize(data);
    }

    private string? BuildFilterItemListJsonLd(IReadOnlyList<Telefon> items)
    {
        var elements = items
            .Where(x => !string.IsNullOrWhiteSpace(x.Slug))
            .Select((phone, index) => new Dictionary<string, object?>
            {
                ["@type"] = "Product",
                ["name"] = BuildPhoneTitle(phone),
                ["url"] = BuildAbsoluteUrl($"/telefon/{phone.Slug}"),
                ["position"] = index + 1
            })
            .ToArray();

        if (elements.Length == 0)
        {
            return null;
        }

        var data = new Dictionary<string, object?>
        {
            ["@context"] = "https://schema.org",
            ["@type"] = "ItemList",
            ["itemListElement"] = elements
        };

        return JsonSerializer.Serialize(data);
    }

    private async Task<IReadOnlyDictionary<string, string>> GetBrandSlugMapAsync(CancellationToken ct)
    {
        return await _memoryCache.GetOrCreateAsync(BrandSlugMapCacheKey, async cacheEntry =>
        {
            cacheEntry.AbsoluteExpirationRelativeToNow = BrandSlugMapCacheDuration;
            cacheEntry.Size = 1;

            var brands = await _telefonRepository.GetDistinctBrandsAsync(ct);
            var map = new Dictionary<string, string>(StringComparer.Ordinal);

            foreach (var brand in brands)
            {
                var slug = BrandSlugHelper.ToSlug(brand);
                if (slug.Length == 0 || map.ContainsKey(slug))
                {
                    continue;
                }

                map[slug] = brand;
            }

            return (IReadOnlyDictionary<string, string>)map;
        }) ?? new Dictionary<string, string>(StringComparer.Ordinal);
    }

    private TelefonListViewModel BuildListViewModel(
        IReadOnlyList<Telefon> items,
        int totalCount,
        int page,
        int totalPages,
        int pageSize,
        IReadOnlyDictionary<string, string> brandMap,
        string basePath,
        string? selectedBrandSlug,
        string? selectedBrand,
        IReadOnlyList<CompareRelatedLinkViewModel> popularComparisons,
        CompareRelatedLinkViewModel? filterPopularComparison,
        string listingTitle,
        string listingDescription,
        bool useFirstPageCanonicalForPagedResults,
        bool noindexPagedResults)
    {
        var brands = brandMap
            .OrderBy(x => x.Value, StringComparer.OrdinalIgnoreCase)
            .Select(x => new BrandLinkViewModel
            {
                Name = x.Value,
                Slug = x.Key,
                Url = $"/telefonlar/marka/{x.Key}",
                IsActive = selectedBrandSlug is not null && string.Equals(selectedBrandSlug, x.Key, StringComparison.Ordinal)
            })
            .ToArray();

        var canonicalPath = useFirstPageCanonicalForPagedResults && page > 1
            ? basePath
            : BuildListingPath(basePath, page);
        var prevPath = page > 1 ? BuildListingPath(basePath, page - 1) : null;
        var nextPath = page < totalPages ? BuildListingPath(basePath, page + 1) : null;
        var robotsMeta = noindexPagedResults && page > 1
            ? "noindex,follow"
            : "index,follow";

        return new TelefonListViewModel
        {
            ListingTitle = listingTitle,
            ListingDescription = listingDescription,
            Items = items,
            Brands = brands,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            Brand = selectedBrand,
            BrandSlug = selectedBrandSlug,
            BasePath = basePath,
            CanonicalUrl = BuildAbsoluteUrl(canonicalPath),
            PrevUrl = prevPath,
            NextUrl = nextPath,
            RobotsMeta = robotsMeta,
            PopularComparisons = popularComparisons,
            FilterPopularComparison = filterPopularComparison
        };
    }

    private static string BuildListingPath(string basePath, int page)
    {
        return page <= 1 ? basePath : $"{basePath}?page={page}";
    }

    private void ApplyListingSeo(TelefonListViewModel viewModel, string title, string description)
    {
        SetSeo(title, description, viewModel.CanonicalUrl);

        ViewData["Robots"] = viewModel.RobotsMeta;
        ViewData["PrevUrl"] = viewModel.PrevUrl is null ? null : BuildAbsoluteUrl(viewModel.PrevUrl);
        ViewData["NextUrl"] = viewModel.NextUrl is null ? null : BuildAbsoluteUrl(viewModel.NextUrl);
    }

    private async Task<IReadOnlyList<CompareRelatedLinkViewModel>> GetBrandPopularComparisonsAsync(
        string brand,
        string brandSlug,
        CancellationToken ct)
    {
        var cacheKey = $"{BrandPopularCompareCacheKeyPrefix}:{brandSlug}";
        if (_memoryCache.TryGetValue<IReadOnlyList<CompareRelatedLinkViewModel>>(cacheKey, out var cached)
            && cached is not null)
        {
            return cached;
        }

        var latestBrandPhones = await _telefonRepository.GetLatestByBrandAsync(brand, take: 5, ct);
        var links = BuildBrandPopularComparisons(latestBrandPhones, maxLinks: 10);

        SetCacheEntry(cacheKey, links, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = BrandPopularCompareCacheDuration,
            Size = 3
        });

        return links;
    }

    private async Task<IReadOnlyList<CompareRelatedLinkViewModel>> GetDetailCompareSuggestionsAsync(
        string slug,
        string currentTitle,
        CancellationToken ct)
    {
        var cacheKey = $"{DetailCompareCacheKeyPrefix}:{slug}";
        if (_memoryCache.TryGetValue<IReadOnlyList<CompareRelatedLinkViewModel>>(cacheKey, out var cached)
            && cached is not null)
        {
            return cached;
        }

        var related = await _telefonService.GetRelatedComparisonLinksAsync([slug], perSlug: 8, totalMax: 8, ct);
        var links = related
            .Select(x => new CompareRelatedLinkViewModel
            {
                Url = x.UrlPath,
                Title = BuildDetailCompareTitle(currentTitle, x),
                ImageUrl = x.OtherImageUrl
            })
            .ToArray();

        SetCacheEntry(cacheKey, links, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = DetailCompareCacheDuration,
            Size = 2
        });

        return links;
    }

    private async Task<IReadOnlyList<CompareRelatedLinkViewModel>> GetDetailTopComparedLinksAsync(
        string slug,
        string currentTitle,
        CancellationToken ct)
    {
        var topCompared = await _telefonService.GetTopComparedLinksAsync(slug, 3, ct);
        return topCompared
            .Select(x => new CompareRelatedLinkViewModel
            {
                Url = x.UrlPath,
                Title = BuildDetailCompareTitle(currentTitle, x),
                ImageUrl = x.OtherImageUrl
            })
            .ToArray();
    }

    private static string BuildDetailCompareTitle(string currentTitle, RelatedComparisonLink relatedLink)
    {
        return $"{currentTitle} vs {(!string.IsNullOrWhiteSpace(relatedLink.OtherTitle) ? relatedLink.OtherTitle : relatedLink.OtherSlug)}";
    }

    private static IReadOnlyList<CompareRelatedLinkViewModel> BuildBrandPopularComparisons(
        IReadOnlyList<Telefon> phones,
        int maxLinks)
    {
        if (phones.Count < 2 || maxLinks <= 0)
        {
            return [];
        }

        var safePhones = phones
            .Where(x => !string.IsNullOrWhiteSpace(x.Slug))
            .Take(5)
            .ToArray();

        if (safePhones.Length < 2)
        {
            return [];
        }

        var results = new List<CompareRelatedLinkViewModel>(Math.Min(maxLinks, 10));
        var pairSet = new HashSet<string>(StringComparer.Ordinal);

        for (var i = 0; i < safePhones.Length && results.Count < maxLinks; i++)
        {
            for (var j = i + 1; j < safePhones.Length && results.Count < maxLinks; j++)
            {
                var leftSlug = safePhones[i].Slug!;
                var rightSlug = safePhones[j].Slug!;

                var canonicalLeft = string.Compare(leftSlug, rightSlug, StringComparison.Ordinal) <= 0 ? leftSlug : rightSlug;
                var canonicalRight = canonicalLeft == leftSlug ? rightSlug : leftSlug;
                var pairKey = $"{canonicalLeft}|{canonicalRight}";
                if (!pairSet.Add(pairKey))
                {
                    continue;
                }

                var leftTitle = canonicalLeft == leftSlug
                    ? BuildPhoneTitle(safePhones[i])
                    : BuildPhoneTitle(safePhones[j]);
                var rightTitle = canonicalRight == rightSlug
                    ? BuildPhoneTitle(safePhones[j])
                    : BuildPhoneTitle(safePhones[i]);

                results.Add(new CompareRelatedLinkViewModel
                {
                    Url = $"/karsilastir/{canonicalLeft}-vs-{canonicalRight}",
                    Title = $"{leftTitle} vs {rightTitle}",
                    ImageUrl = null
                });
            }
        }

        return results;
    }

    private static CompareRelatedLinkViewModel? BuildFilterPopularComparison(IReadOnlyList<Telefon> items)
    {
        var pair = items
            .Where(x => !string.IsNullOrWhiteSpace(x.Slug))
            .Take(2)
            .ToArray();

        if (pair.Length < 2)
        {
            return null;
        }

        var firstSlug = pair[0].Slug!;
        var secondSlug = pair[1].Slug!;
        var canonicalLeft = string.Compare(firstSlug, secondSlug, StringComparison.Ordinal) <= 0 ? firstSlug : secondSlug;
        var canonicalRight = canonicalLeft == firstSlug ? secondSlug : firstSlug;
        var leftTitle = canonicalLeft == firstSlug ? BuildPhoneTitle(pair[0]) : BuildPhoneTitle(pair[1]);
        var rightTitle = canonicalRight == secondSlug ? BuildPhoneTitle(pair[1]) : BuildPhoneTitle(pair[0]);

        return new CompareRelatedLinkViewModel
        {
            Url = $"/karsilastir/{canonicalLeft}-vs-{canonicalRight}",
            Title = $"{leftTitle} vs {rightTitle}",
            ImageUrl = null
        };
    }

    private void SetCacheEntry<T>(string key, T value, MemoryCacheEntryOptions options)
    {
        try
        {
            _memoryCache.Set(key, value, options);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("size", StringComparison.OrdinalIgnoreCase))
        {
            _memoryCache.Set(key, value, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = options.AbsoluteExpirationRelativeToNow,
                SlidingExpiration = options.SlidingExpiration
            });
        }
    }
}
