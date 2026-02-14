using Kiyaslasana.BL.Abstractions;
using Kiyaslasana.PL.Infrastructure;
using Kiyaslasana.PL.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace Kiyaslasana.PL.Controllers;

public sealed class HomeController : SeoControllerBase
{
    private readonly ITelefonService _telefonService;

    public HomeController(ITelefonService telefonService)
    {
        _telefonService = telefonService;
    }

    [HttpGet("/")]
    [OutputCache(PolicyName = OutputCachePolicyNames.AnonymousOneHour)]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var latest = await _telefonService.GetLatestAsync(8, ct);

        SetSeo(
            title: "Kiyaslasana - Telefon Karsilastirma",
            description: "Telefon modellerini incele, ozelliklerini karsilastir ve sana uygun cihazi bul.",
            canonicalUrl: BuildAbsoluteUrl("/"));

        ViewData["Nav"] = "home";

        return View(new HomeViewModel
        {
            LatestPhones = latest
        });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [HttpGet("/hata")]
    public IActionResult Error()
    {
        return View();
    }
}
