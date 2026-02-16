using System.Text.Json;
using Kiyaslasana.BL.Abstractions;
using Kiyaslasana.BL.Helpers;
using Kiyaslasana.BL.Services;
using Kiyaslasana.PL.Infrastructure;
using Kiyaslasana.PL.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace Kiyaslasana.PL.Controllers;

public sealed class BlogController : SeoControllerBase
{
    private const int BlogListPageSize = 12;

    private readonly IBlogPostService _blogPostService;

    public BlogController(IBlogPostService blogPostService)
    {
        _blogPostService = blogPostService;
    }

    [HttpGet("/blog")]
    [OutputCache(PolicyName = OutputCachePolicyNames.AnonymousOneHour, VaryByQueryKeys = ["page"])]
    public async Task<IActionResult> Index([FromQuery] int page = 1, CancellationToken ct = default)
    {
        if (page < 1)
        {
            return NotFound();
        }

        var (items, totalCount) = await _blogPostService.GetPublishedPagedAsync(page, BlogListPageSize, ct);
        var paging = PagingHelper.Normalize(page, BlogListPageSize, totalCount);
        var basePath = "/blog";
        var canonicalPath = BuildListingPath(basePath, paging.Page);
        var prevPath = paging.Page > 1 ? BuildListingPath(basePath, paging.Page - 1) : null;
        var nextPath = paging.Page < paging.TotalPages ? BuildListingPath(basePath, paging.Page + 1) : null;

        SetSeo(
            title: paging.Page > 1 ? $"Blog - Sayfa {paging.Page}" : "Blog",
            description: "Telefon dunyasina dair inceleme, karsilastirma ve rehber yazilari.",
            canonicalUrl: BuildAbsoluteUrl(canonicalPath));

        ViewData["Nav"] = "blog";
        ViewData["Robots"] = paging.Page >= 2 ? "noindex,follow" : "index,follow";
        ViewData["PrevUrl"] = prevPath is null ? null : BuildAbsoluteUrl(prevPath);
        ViewData["NextUrl"] = nextPath is null ? null : BuildAbsoluteUrl(nextPath);

        return View(new BlogListViewModel
        {
            Items = items,
            Page = paging.Page,
            PageSize = paging.PageSize,
            TotalCount = totalCount,
            TotalPages = paging.TotalPages,
            CanonicalUrl = BuildAbsoluteUrl(canonicalPath),
            PrevUrl = prevPath,
            NextUrl = nextPath,
            RobotsMeta = paging.Page >= 2 ? "noindex,follow" : "index,follow"
        });
    }

    [HttpGet("/blog/{slug}")]
    [OutputCache(PolicyName = OutputCachePolicyNames.AnonymousOneHour, VaryByRouteValueNames = ["slug"])]
    public async Task<IActionResult> Detail(string slug, CancellationToken ct)
    {
        var post = await _blogPostService.GetPublishedBySlugAsync(slug, ct);
        if (post is null)
        {
            return NotFound();
        }

        var canonicalPath = $"/blog/{post.Slug}";
        var canonicalUrl = BuildAbsoluteUrl(canonicalPath);
        var title = !string.IsNullOrWhiteSpace(post.MetaTitle) ? post.MetaTitle : post.Title;
        var description = !string.IsNullOrWhiteSpace(post.MetaDescription)
            ? post.MetaDescription
            : post.Excerpt ?? "Blog yazisi";

        SetSeo(title, description, canonicalUrl);
        ViewData["Nav"] = "blog";
        ViewData["OgTitle"] = title;
        ViewData["OgDescription"] = description;
        ViewData["OgUrl"] = canonicalUrl;
        ViewData["TwitterCard"] = "summary";
        ViewData["TwitterTitle"] = title;
        ViewData["TwitterDescription"] = description;
        ViewData["TwitterUrl"] = canonicalUrl;

        var links = await _blogPostService.BuildInternalLinksAsync(post, ct);

        return View(new BlogDetailViewModel
        {
            Post = post,
            BlogPostingJsonLd = BuildBlogPostingJsonLd(post, canonicalUrl, description),
            InternalLinks = links
        });
    }

    private static string BuildListingPath(string basePath, int page)
    {
        return page <= 1 ? basePath : $"{basePath}?page={page}";
    }

    private static string BuildBlogPostingJsonLd(Kiyaslasana.EL.Entities.BlogPost post, string canonicalUrl, string description)
    {
        var data = new Dictionary<string, object?>
        {
            ["@context"] = "https://schema.org",
            ["@type"] = "BlogPosting",
            ["headline"] = post.Title,
            ["datePublished"] = post.PublishedAt?.ToString("O"),
            ["author"] = new Dictionary<string, object?>
            {
                ["@type"] = "Person",
                ["name"] = BlogPostService.GetDefaultAuthorName()
            },
            ["mainEntityOfPage"] = canonicalUrl,
            ["description"] = description,
            ["url"] = canonicalUrl
        };

        return JsonSerializer.Serialize(data);
    }
}
