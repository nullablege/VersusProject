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
}
