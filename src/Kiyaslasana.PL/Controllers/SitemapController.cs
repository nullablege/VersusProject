using System.Xml.Linq;
using Kiyaslasana.BL.Abstractions;
using Kiyaslasana.PL.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace Kiyaslasana.PL.Controllers;

public sealed class SitemapController : Controller
{
    private readonly ITelefonService _telefonService;

    public SitemapController(ITelefonService telefonService)
    {
        _telefonService = telefonService;
    }

    [HttpGet("/sitemap.xml")]
    [OutputCache(PolicyName = OutputCachePolicyNames.AnonymousOneHour)]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var phones = await _telefonService.GetLatestAsync(200, ct);
        var baseUrl = $"{Request.Scheme}://{Request.Host}";

        XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
        var urlset = new XElement(ns + "urlset",
            new XElement(ns + "url", new XElement(ns + "loc", baseUrl + "/")),
            new XElement(ns + "url", new XElement(ns + "loc", baseUrl + "/telefonlar")));

        foreach (var phone in phones.Where(x => !string.IsNullOrWhiteSpace(x.Slug)))
        {
            urlset.Add(new XElement(ns + "url", new XElement(ns + "loc", $"{baseUrl}/telefon/{phone.Slug}")));
        }

        var doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), urlset);
        return Content(doc.ToString(), "application/xml");
    }
}
