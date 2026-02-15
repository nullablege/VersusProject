using System.Text.Json;
using Kiyaslasana.BL.Abstractions;
using Kiyaslasana.PL.Infrastructure;
using Kiyaslasana.PL.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Kiyaslasana.PL.Controllers;

public sealed class KarsilastirController : SeoControllerBase
{
    private readonly ITelefonService _telefonService;

    public KarsilastirController(ITelefonService telefonService)
    {
        _telefonService = telefonService;
    }

    [HttpGet("/karsilastir")]
    public async Task<IActionResult> Index([FromQuery] string? first, CancellationToken ct)
    {
        var viewModel = await BuildBuilderViewModelAsync(
            isAuthenticated: User.Identity?.IsAuthenticated ?? false,
            requestedSlugs: string.IsNullOrWhiteSpace(first) ? [] : [_telefonService.NormalizeSlug(first)],
            ct);

        SetSeo(
            title: "Telefon Karsilastirma Builder",
            description: "Karsilastirmak istedigin telefon sluglarini sec ve karsilastirma sayfasina git.",
            canonicalUrl: BuildAbsoluteUrl("/karsilastir"));
        ViewData["Robots"] = "noindex,follow";
        ViewData["Nav"] = "karsilastir";

        return View("~/Views/Compare/Index.cshtml", viewModel);
    }

    [HttpPost("/karsilastir")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index([FromForm] string[] slugs, CancellationToken ct)
    {
        var isAuthenticated = User.Identity?.IsAuthenticated ?? false;
        var maxAllowed = isAuthenticated ? 4 : 2;

        var normalized = new List<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var slug in slugs)
        {
            var current = _telefonService.NormalizeSlug(slug);
            if (current.Length == 0)
            {
                continue;
            }

            if (seen.Add(current))
            {
                normalized.Add(current);
            }
        }

        if (normalized.Count < 2)
        {
            ModelState.AddModelError(string.Empty, "Karsilastirma icin en az 2 telefon slug girin.");
        }

        if (normalized.Count > maxAllowed)
        {
            ModelState.AddModelError(string.Empty, $"Bu hesap tipi icin en fazla {maxAllowed} telefon karsilastirilabilir.");
        }

        if (!ModelState.IsValid)
        {
            var viewModel = await BuildBuilderViewModelAsync(isAuthenticated, normalized, ct);
            SetSeo(
                title: "Telefon Karsilastirma Builder",
                description: "Karsilastirmak istedigin telefon sluglarini sec ve karsilastirma sayfasina git.",
                canonicalUrl: BuildAbsoluteUrl("/karsilastir"));
            ViewData["Robots"] = "noindex,follow";
            ViewData["Nav"] = "karsilastir";
            return View("~/Views/Compare/Index.cshtml", viewModel);
        }

        var canonicalSlugs = normalized.OrderBy(x => x, StringComparer.Ordinal).ToArray();
        var comparePath = "/karsilastir/" + string.Join("-vs-", canonicalSlugs);
        return Redirect(comparePath);
    }

    [HttpGet("/karsilastir/{slug1:regex(^(?!.*-vs-)[A-Za-z0-9-]+$)}-vs-{slug2:regex(^(?!.*-vs-)[A-Za-z0-9-]+$)}")]
    public async Task<IActionResult> CompareTwo(string slug1, string slug2, CancellationToken ct)
    {
        return await RenderCompareAsync([slug1, slug2], isSeoIndexable: true, ct);
    }

    [HttpGet("/karsilastir/{slug1:regex(^(?!.*-vs-)[A-Za-z0-9-]+$)}-vs-{slug2:regex(^(?!.*-vs-)[A-Za-z0-9-]+$)}-vs-{slug3:regex(^(?!.*-vs-)[A-Za-z0-9-]+$)}")]
    public async Task<IActionResult> CompareThree(string slug1, string slug2, string slug3, CancellationToken ct)
    {
        return await RenderCompareAsync([slug1, slug2, slug3], isSeoIndexable: false, ct);
    }

    [HttpGet("/karsilastir/{slug1:regex(^(?!.*-vs-)[A-Za-z0-9-]+$)}-vs-{slug2:regex(^(?!.*-vs-)[A-Za-z0-9-]+$)}-vs-{slug3:regex(^(?!.*-vs-)[A-Za-z0-9-]+$)}-vs-{slug4:regex(^(?!.*-vs-)[A-Za-z0-9-]+$)}")]
    public async Task<IActionResult> CompareFour(string slug1, string slug2, string slug3, string slug4, CancellationToken ct)
    {
        return await RenderCompareAsync([slug1, slug2, slug3, slug4], isSeoIndexable: false, ct);
    }

    private async Task<IActionResult> RenderCompareAsync(IReadOnlyList<string> slugs, bool isSeoIndexable, CancellationToken ct)
    {
        var isAuthenticated = User.Identity?.IsAuthenticated ?? false;
        var resolve = await _telefonService.ResolveCompareAsync(slugs, isAuthenticated, ct);

        if (!resolve.IsValid)
        {
            if (resolve.ErrorMessage?.Contains("limiti asildi", StringComparison.OrdinalIgnoreCase) == true)
            {
                return BadRequest(resolve.ErrorMessage);
            }

            return NotFound();
        }

        var canonicalPath = "/karsilastir/" + string.Join("-vs-", resolve.CanonicalSlugs);
        var indexable = isSeoIndexable && resolve.CanonicalSlugs.Count == 2;
        var comparisonTitle = string.Join(" vs ", resolve.Phones.Select(BuildPhoneTitle));
        var pageTitle = comparisonTitle.Length == 0 ? "Telefon Karsilastirma" : $"{comparisonTitle} Karsilastirma";
        var metaDescription = indexable
            ? $"{comparisonTitle} ozelliklerini yan yana karsilastir."
            : "Coklu telefon karsilastirmasi sonucu.";

        SetSeo(
            title: pageTitle,
            description: metaDescription,
            canonicalUrl: BuildAbsoluteUrl(canonicalPath));

        ViewData["Robots"] = indexable ? "index,follow" : "noindex,follow";
        ViewData["Nav"] = "karsilastir";

        return View("~/Views/Compare/Compare.cshtml", new CompareViewModel
        {
            Phones = resolve.Phones,
            IsSeoIndexable = indexable,
            CanonicalUrl = BuildAbsoluteUrl(canonicalPath),
            PageTitle = pageTitle,
            MetaDescription = metaDescription,
            BreadcrumbJsonLd = indexable ? BuildBreadcrumbJsonLd(canonicalPath, resolve.Phones) : null
        });
    }

    private async Task<CompareBuilderViewModel> BuildBuilderViewModelAsync(
        bool isAuthenticated,
        IReadOnlyList<string> requestedSlugs,
        CancellationToken ct)
    {
        var maxAllowed = isAuthenticated ? 4 : 2;
        var slots = Enumerable.Repeat(string.Empty, maxAllowed).ToArray();

        for (var i = 0; i < Math.Min(requestedSlugs.Count, maxAllowed); i++)
        {
            slots[i] = requestedSlugs[i];
        }

        var suggestions = await _telefonService.GetLatestAsync(200, ct);
        return new CompareBuilderViewModel
        {
            MaxAllowed = maxAllowed,
            SlugInputs = slots,
            SuggestedPhones = suggestions
        };
    }

    private static string BuildPhoneTitle(Kiyaslasana.EL.Entities.Telefon phone)
    {
        return string.Join(' ', new[] { phone.Marka, phone.ModelAdi }.Where(x => !string.IsNullOrWhiteSpace(x))).Trim();
    }

    private string BuildBreadcrumbJsonLd(string canonicalPath, IReadOnlyList<Kiyaslasana.EL.Entities.Telefon> phones)
    {
        var compareTitle = string.Join(" vs ", phones.Select(BuildPhoneTitle));
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
                    name = "Karsilastir",
                    item = BuildAbsoluteUrl("/karsilastir")
                },
                new
                {
                    @type = "ListItem",
                    position = 3,
                    name = compareTitle,
                    item = BuildAbsoluteUrl(canonicalPath)
                }
            }
        };

        return JsonSerializer.Serialize(data);
    }
}
