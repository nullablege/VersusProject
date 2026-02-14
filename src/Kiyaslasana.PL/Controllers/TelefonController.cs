using System.Text.Json;
using Kiyaslasana.BL.Abstractions;
using Kiyaslasana.EL.Entities;
using Kiyaslasana.PL.Infrastructure;
using Kiyaslasana.PL.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace Kiyaslasana.PL.Controllers;

public sealed class TelefonController : SeoControllerBase
{
    private readonly ITelefonService _telefonService;

    public TelefonController(ITelefonService telefonService)
    {
        _telefonService = telefonService;
    }

    [HttpGet("/telefonlar")]
    [OutputCache(PolicyName = OutputCachePolicyNames.AnonymousOneHour)]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var phones = await _telefonService.GetLatestAsync(48, ct);

        SetSeo(
            title: "Tum Telefonlar",
            description: "Tum telefon modellerini tek listede gor ve karsilastirma icin secim yap.",
            canonicalUrl: BuildAbsoluteUrl("/telefonlar"));

        ViewData["Nav"] = "telefonlar";

        return View(new TelefonListViewModel
        {
            Phones = phones
        });
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
}
