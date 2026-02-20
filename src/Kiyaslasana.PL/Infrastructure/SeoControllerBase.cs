using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;

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
        var services = HttpContext.RequestServices;
        var configuration = services.GetService<IConfiguration>();
        var hostEnvironment = services.GetService<IHostEnvironment>();

        var configuredBaseUrl = NormalizePublicBaseUrl(configuration?["Seo:PublicBaseUrl"]);

        // Security: in non-development environments we require an explicit public base URL.
        // This avoids building canonical/SEO URLs from potentially spoofed request host values.
        var baseUrl = configuredBaseUrl ?? (hostEnvironment?.IsDevelopment() == true
            ? $"{Request.Scheme}://{Request.Host}"
            : throw new InvalidOperationException("Seo:PublicBaseUrl must be configured outside Development."));

        var safeRelativePath = relativePath.StartsWith('/') ? relativePath : "/" + relativePath;
        var pathBase = HttpContext.Request.PathBase.Value ?? string.Empty;
        var combinedPath = CombinePathSegments(pathBase, safeRelativePath);

        return $"{baseUrl}{combinedPath}";
    }

    protected void SetPublicCacheControl(int maxAgeSeconds)
    {
        var safeMaxAgeSeconds = Math.Max(maxAgeSeconds, 0);
        Response.Headers[HeaderNames.CacheControl] = $"public, max-age={safeMaxAgeSeconds}";
    }

    private static string? NormalizePublicBaseUrl(string? publicBaseUrl)
    {
        if (string.IsNullOrWhiteSpace(publicBaseUrl))
        {
            return null;
        }

        if (!Uri.TryCreate(publicBaseUrl, UriKind.Absolute, out var baseUri))
        {
            return null;
        }

        if (baseUri.Scheme != Uri.UriSchemeHttp && baseUri.Scheme != Uri.UriSchemeHttps)
        {
            return null;
        }

        var origin = $"{baseUri.Scheme}://{baseUri.Authority}";
        var basePath = baseUri.AbsolutePath.TrimEnd('/');

        if (basePath.Length == 0 || basePath == "/")
        {
            return origin;
        }

        return $"{origin}{basePath}";
    }

    private static string CombinePathSegments(string pathBase, string relativePath)
    {
        var normalizedBase = pathBase.Trim('/');
        var normalizedRelative = relativePath.TrimStart('/');

        if (normalizedBase.Length == 0 && normalizedRelative.Length == 0)
        {
            return "/";
        }

        if (normalizedBase.Length == 0)
        {
            return "/" + normalizedRelative;
        }

        if (normalizedRelative.Length == 0)
        {
            return "/" + normalizedBase;
        }

        return "/" + normalizedBase + "/" + normalizedRelative;
    }
}
