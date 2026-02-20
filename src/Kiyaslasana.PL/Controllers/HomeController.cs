using Kiyaslasana.BL.Abstractions;
using Kiyaslasana.PL.Infrastructure;
using Kiyaslasana.PL.Models;
using Kiyaslasana.PL.ViewModels;
using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Kiyaslasana.PL.Controllers;

public sealed class HomeController : SeoControllerBase
{
    private readonly ITelefonService _telefonService;
    private readonly ILogger<HomeController> _logger;

    public HomeController(ITelefonService telefonService, ILogger<HomeController>? logger = null)
    {
        _telefonService = telefonService;
        _logger = logger ?? NullLogger<HomeController>.Instance;
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
    [HttpGet("/error")]
    [HttpGet("/error/{statusCode:int?}")]
    [HttpGet("/hata")]
    [HttpGet("/hata/{statusCode:int?}")]
    public IActionResult Error(int? statusCode = null)
    {
        var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
        if (exceptionFeature?.Error is not null)
        {
            _logger.LogError(
                exceptionFeature.Error,
                "Unhandled exception for path {OriginalPath}",
                exceptionFeature.Path);
        }

        var acceptsJson = Request.GetTypedHeaders().Accept?.Any(x =>
            x.MediaType.HasValue
            && x.MediaType.Value.Contains("json", StringComparison.OrdinalIgnoreCase)) == true;
        var isJsonContent = Request.ContentType?.Contains("json", StringComparison.OrdinalIgnoreCase) == true;
        var isApiRequest = exceptionFeature?.Path?.StartsWith("/api", StringComparison.OrdinalIgnoreCase) == true
            || acceptsJson
            || isJsonContent;
        if (isApiRequest)
        {
            return Problem(
                title: "Beklenmeyen bir hata olustu.",
                statusCode: StatusCodes.Status500InternalServerError,
                detail: "Istek islenirken sunucu tarafinda bir hata olustu.");
        }

        var resolvedStatusCode = statusCode ?? HttpContext.Response.StatusCode;
        if (resolvedStatusCode < 400)
        {
            resolvedStatusCode = StatusCodes.Status500InternalServerError;
        }

        HttpContext.Response.StatusCode = resolvedStatusCode;

        ViewData["StatusCode"] = resolvedStatusCode;
        ViewData["StatusMessage"] = resolvedStatusCode switch
        {
            404 => "Istenen sayfa bulunamadi.",
            500 => "Sunucuda beklenmeyen bir hata olustu.",
            _ => "Bir hata olustu."
        };

        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
            StatusCode = resolvedStatusCode
        });
    }
}
