using Microsoft.AspNetCore.Mvc;

namespace Kiyaslasana.PL.Infrastructure;

public abstract class SeoControllerBase : Controller
{
    protected void SetSeo(string title, string description, string? canonicalUrl = null)
    {
        ViewData["Title"] = title;
        ViewData["MetaDescription"] = description;

        if (!string.IsNullOrWhiteSpace(canonicalUrl))
        {
            ViewData["Canonical"] = canonicalUrl;
        }
    }

    protected string BuildAbsoluteUrl(string relativePath)
    {
        var pathBase = HttpContext.Request.PathBase.HasValue ? HttpContext.Request.PathBase.Value : string.Empty;
        var safePath = relativePath.StartsWith('/') ? relativePath : "/" + relativePath;
        return $"{Request.Scheme}://{Request.Host}{pathBase}{safePath}";
    }
}
