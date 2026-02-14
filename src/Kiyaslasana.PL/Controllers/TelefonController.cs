using System.Text.Json;
using Kiyaslasana.BL.Abstractions;
using Kiyaslasana.BL.Helpers;
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
    private static readonly TimeSpan BrandSlugMapCacheDuration = TimeSpan.FromHours(24);

    private readonly ITelefonService _telefonService;
    private readonly ITelefonRepository _telefonRepository;
    private readonly IMemoryCache _memoryCache;

    public TelefonController(
        ITelefonService telefonService,
        ITelefonRepository telefonRepository,
        IMemoryCache memoryCache)
    {
        _telefonService = telefonService;
        _telefonRepository = telefonRepository;
        _memoryCache = memoryCache;
    }

    [HttpGet("/telefonlar")]
    [OutputCache(PolicyName = OutputCachePolicyNames.AnonymousOneHour, VaryByQueryKeys = ["page"])]
    public async Task<IActionResult> Index([FromQuery] int page = 1, CancellationToken ct = default)
    {
        var requestedPage = Math.Max(page, 1);
        var initialSkip = (requestedPage - 1) * ListingPageSize;

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
            selectedBrand: null);

        ApplyListingSeo(viewModel, "Tum Telefonlar", "Tum telefon modellerini sayfali listede gor ve karsilastirma icin secim yap.");

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
        var viewModel = BuildListViewModel(
            items: items,
            totalCount: totalCount,
            page: paging.Page,
            totalPages: paging.TotalPages,
            pageSize: paging.PageSize,
            brandMap: brandMap,
            basePath: basePath,
            selectedBrandSlug: normalizedBrandSlug,
            selectedBrand: brand);

        ApplyListingSeo(
            viewModel,
            $"{brand} Telefonlari",
            $"{brand} marka telefon modellerini sayfali listede gor ve karsilastirma icin secim yap.");

        ViewData["Nav"] = "telefonlar";
        return View("Index", viewModel);
    }

    [HttpGet("/telefon/{slug}")]
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

        SetSeo(
            title: title,
            description: $"{title} teknik ozellikleri, fiyat bilgisi ve karsilastirma detaylari.",
            canonicalUrl: BuildAbsoluteUrl($"/telefon/{normalizedSlug}"));

        ViewData["Nav"] = "telefonlar";

        return View(new TelefonDetailViewModel
        {
            Telefon = phone,
            ProductJsonLd = BuildProductJsonLd(phone, normalizedSlug),
            BreadcrumbJsonLd = BuildBreadcrumbJsonLd(phone, normalizedSlug)
        });
    }

    private static string BuildPhoneTitle(Telefon phone)
    {
        return string.Join(' ', new[] { phone.Marka, phone.ModelAdi }.Where(x => !string.IsNullOrWhiteSpace(x))).Trim();
    }

    private string BuildProductJsonLd(Telefon phone, string slug)
    {
        var title = BuildPhoneTitle(phone);
        var data = new
        {
            @context = "https://schema.org",
            @type = "Product",
            name = title,
            image = phone.ResimUrl,
            brand = new
            {
                @type = "Brand",
                name = phone.Marka
            },
            sku = phone.Slug,
            offers = new
            {
                @type = "Offer",
                priceCurrency = "TRY",
                price = phone.Fiyat,
                availability = "https://schema.org/InStock",
                url = BuildAbsoluteUrl($"/telefon/{slug}")
            }
        };

        return JsonSerializer.Serialize(data);
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

    private async Task<IReadOnlyDictionary<string, string>> GetBrandSlugMapAsync(CancellationToken ct)
    {
        return await _memoryCache.GetOrCreateAsync(BrandSlugMapCacheKey, async cacheEntry =>
        {
            cacheEntry.AbsoluteExpirationRelativeToNow = BrandSlugMapCacheDuration;

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
        string? selectedBrand)
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

        var canonicalPath = BuildListingPath(basePath, page);
        var prevPath = page > 1 ? BuildListingPath(basePath, page - 1) : null;
        var nextPath = page < totalPages ? BuildListingPath(basePath, page + 1) : null;

        return new TelefonListViewModel
        {
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
            RobotsMeta = page >= 2 ? "noindex,follow" : "index,follow"
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
}
