using Kiyaslasana.PL.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace Kiyaslasana.PL.Controllers;

public sealed class RobotsController : SeoControllerBase
{
    [HttpGet("/robots.txt")]
    [OutputCache(PolicyName = OutputCachePolicyNames.AnonymousOneDay)]
    public IActionResult Index()
    {
        var body = string.Join('\n',
        [
            "User-agent: *",
            "Disallow:",
            "Disallow: /*?*sort=",
            "Disallow: /telefonlar/marka/*?*page=",
            "Disallow: /telefonlar/*?*page=",
            $"Sitemap: {BuildAbsoluteUrl("/sitemap.xml")}"
        ]);

        return Content(body, "text/plain; charset=utf-8");
    }
}
