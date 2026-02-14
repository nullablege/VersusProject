using Kiyaslasana.BL.Abstractions;
using Kiyaslasana.PL.Infrastructure;
using Kiyaslasana.PL.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace Kiyaslasana.PL.Controllers;

public sealed class KarsilastirController : SeoControllerBase
{
    private readonly ITelefonService _telefonService;

    public KarsilastirController(ITelefonService telefonService)
    {
        _telefonService = telefonService;
    }

    [HttpGet("/karsilastir")]
    [OutputCache(PolicyName = OutputCachePolicyNames.AnonymousOneHour)]
    public async Task<IActionResult> Builder([FromQuery] string? first, CancellationToken ct)
    {
        var viewModel = await BuildBuilderViewModelAsync(
            isAuthenticated: User.Identity?.IsAuthenticated ?? false,
            requestedSlugs: string.IsNullOrWhiteSpace(first) ? [] : [_telefonService.NormalizeSlug(first)],
            ct);

        SetSeo(
            title: "Telefon Karsilastirma Builder",
            description: "Karsilastirmak istedigin telefon sluglarini sec ve karsilastirma sayfasina git.",
            canonicalUrl: BuildAbsoluteUrl("/karsilastir"));
        ViewData["Nav"] = "karsilastir";

        return View(viewModel);
    }

    [HttpPost("/karsilastir")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Builder([FromForm] string[] slugs, CancellationToken ct)
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
            ViewData["Nav"] = "karsilastir";
            return View(viewModel);
        }

        var canonicalSlugs = normalized.OrderBy(x => x, StringComparer.Ordinal).ToArray();
        var comparePath = "/karsilastir/" + string.Join("-vs-", canonicalSlugs);
        return Redirect(comparePath);
    }

    [HttpGet("/karsilastir/{slugs}")]
    [OutputCache(PolicyName = OutputCachePolicyNames.AnonymousOneDay, VaryByRouteValueNames = ["slugs"])]
    public async Task<IActionResult> Index(string slugs, CancellationToken ct)
    {
        var isAuthenticated = User.Identity?.IsAuthenticated ?? false;
        var parse = _telefonService.ParseCompareSlugs(slugs, isAuthenticated);

        if (!parse.IsValid)
        {
            return BadRequest(parse.ErrorMessage);
        }

        var phones = await _telefonService.GetBySlugsAsync(parse.CanonicalSlugs, ct);
        if (phones.Count != parse.CanonicalSlugs.Count || phones.Count < 2)
        {
            return NotFound();
        }

        var canonicalPath = "/karsilastir/" + string.Join("-vs-", parse.CanonicalSlugs);

        SetSeo(
            title: "Telefon Karsilastirma",
            description: "Secilen telefon modellerini yan yana karsilastir.",
            canonicalUrl: BuildAbsoluteUrl(canonicalPath));

        ViewData["Nav"] = "karsilastir";

        return View(new KarsilastirViewModel
        {
            Phones = phones,
            CanonicalComparePath = canonicalPath,
            MaxAllowed = parse.MaxAllowed
        });
    }

    private async Task<KarsilastirBuilderViewModel> BuildBuilderViewModelAsync(
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
        return new KarsilastirBuilderViewModel
        {
            MaxAllowed = maxAllowed,
            SlugInputs = slots,
            SuggestedPhones = suggestions
        };
    }
}
