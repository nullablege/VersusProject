using System.Xml.Linq;
using Kiyaslasana.BL.Abstractions;
using Kiyaslasana.PL.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace Kiyaslasana.PL.Controllers;

public sealed class SitemapController : SeoControllerBase
{
    private const int TelefonSitemapPageSize = 40000;

    private readonly ITelefonSitemapQuery _telefonSitemapQuery;
    private readonly IBlogPostService _blogPostService;

    public SitemapController(ITelefonSitemapQuery telefonSitemapQuery, IBlogPostService blogPostService)
    {
        _telefonSitemapQuery = telefonSitemapQuery;
        _blogPostService = blogPostService;
    }

    [HttpGet("/sitemap.xml")]
    [OutputCache(PolicyName = OutputCachePolicyNames.AnonymousOneDay)]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var baseUrl = GetBaseUrl();
        var totalCount = await _telefonSitemapQuery.CountAsync(ct);
        var totalPages = (int)Math.Ceiling(totalCount / (double)TelefonSitemapPageSize);

        XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
        var sitemapIndex = new XElement(ns + "sitemapindex");

        sitemapIndex.Add(new XElement(ns + "sitemap",
            new XElement(ns + "loc", $"{baseUrl}/sitemaps/static.xml")));
        sitemapIndex.Add(new XElement(ns + "sitemap",
            new XElement(ns + "loc", $"{baseUrl}/sitemaps/markalar.xml")));
        sitemapIndex.Add(new XElement(ns + "sitemap",
            new XElement(ns + "loc", $"{baseUrl}/sitemaps/blog.xml")));

        for (var page = 1; page <= totalPages; page++)
        {
            sitemapIndex.Add(new XElement(ns + "sitemap",
                new XElement(ns + "loc", $"{baseUrl}/sitemaps/telefonlar-{page}.xml")));
        }

        var doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), sitemapIndex);
        return Content(doc.ToString(SaveOptions.DisableFormatting), "application/xml; charset=utf-8");
    }

    [HttpGet("/sitemaps/static.xml")]
    [OutputCache(PolicyName = OutputCachePolicyNames.AnonymousOneDay)]
    public IActionResult StaticSitemap()
    {
        var baseUrl = GetBaseUrl();

        XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
        var urlset = new XElement(ns + "urlset",
            new XElement(ns + "url", new XElement(ns + "loc", baseUrl + "/")),
            new XElement(ns + "url", new XElement(ns + "loc", baseUrl + "/telefonlar")),
            new XElement(ns + "url", new XElement(ns + "loc", baseUrl + "/blog")),
            new XElement(ns + "url", new XElement(ns + "loc", baseUrl + "/karsilastir")));

        var doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), urlset);
        return Content(doc.ToString(SaveOptions.DisableFormatting), "application/xml; charset=utf-8");
    }

    [HttpGet("/sitemaps/blog.xml")]
    [OutputCache(PolicyName = OutputCachePolicyNames.AnonymousOneDay)]
    public async Task<IActionResult> BlogSitemap(CancellationToken ct)
    {
        var baseUrl = GetBaseUrl();
        var posts = await _blogPostService.GetPublishedSitemapItemsAsync(ct);

        XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
        var urlset = new XElement(ns + "urlset");

        foreach (var post in posts.Where(x => !string.IsNullOrWhiteSpace(x.Slug)))
        {
            var url = new XElement(ns + "url",
                new XElement(ns + "loc", $"{baseUrl}/blog/{post.Slug}"));

            if (post.UpdatedAt != default)
            {
                url.Add(new XElement(ns + "lastmod", post.UpdatedAt.UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ssZ")));
            }

            urlset.Add(url);
        }

        var doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), urlset);
        return Content(doc.ToString(SaveOptions.DisableFormatting), "application/xml; charset=utf-8");
    }

    [HttpGet("/sitemaps/markalar.xml")]
    [OutputCache(PolicyName = OutputCachePolicyNames.AnonymousOneDay)]
    public async Task<IActionResult> MarkaSitemap(CancellationToken ct)
    {
        var baseUrl = GetBaseUrl();
        var brandSlugs = await _telefonSitemapQuery.GetBrandSlugsAsync(ct);

        XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
        var urlset = new XElement(ns + "urlset");

        foreach (var brandSlug in brandSlugs)
        {
            urlset.Add(new XElement(ns + "url",
                new XElement(ns + "loc", $"{baseUrl}/telefonlar/marka/{brandSlug}")));
        }

        var doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), urlset);
        return Content(doc.ToString(SaveOptions.DisableFormatting), "application/xml; charset=utf-8");
    }

    [HttpGet("/sitemaps/telefonlar-{page:int}.xml")]
    [OutputCache(PolicyName = OutputCachePolicyNames.AnonymousOneDay, VaryByRouteValueNames = ["page"])]
    public async Task<IActionResult> TelefonlarSitemap(int page, CancellationToken ct)
    {
        if (page < 1)
        {
            return NotFound();
        }

        var totalCount = await _telefonSitemapQuery.CountAsync(ct);
        var totalPages = (int)Math.Ceiling(totalCount / (double)TelefonSitemapPageSize);

        if (page > totalPages || totalPages == 0)
        {
            return NotFound();
        }

        var skip = (page - 1) * TelefonSitemapPageSize;
        var slugs = await _telefonSitemapQuery.GetSlugsPageAsync(skip, TelefonSitemapPageSize, ct);

        if (slugs.Count == 0)
        {
            return NotFound();
        }

        var baseUrl = GetBaseUrl();

        XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
        var urlset = new XElement(ns + "urlset");

        foreach (var slug in slugs)
        {
            urlset.Add(new XElement(ns + "url",
                new XElement(ns + "loc", $"{baseUrl}/telefon/{slug}")));
        }

        var doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), urlset);
        return Content(doc.ToString(SaveOptions.DisableFormatting), "application/xml; charset=utf-8");
    }

    private string GetBaseUrl()
    {
        var rootUrl = BuildAbsoluteUrl("/");
        return rootUrl.EndsWith('/') ? rootUrl[..^1] : rootUrl;
    }
}
