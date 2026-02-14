using Kiyaslasana.PL.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace Kiyaslasana.PL.Controllers;

public sealed class RobotsController : Controller
{
    [HttpGet("/robots.txt")]
    [OutputCache(PolicyName = OutputCachePolicyNames.AnonymousOneDay)]
    public IActionResult Index()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var body = string.Join('\n',
        [
            "User-agent: *",
            "Disallow:",
            $"Sitemap: {baseUrl}/sitemap.xml"
        ]);

        return Content(body, "text/plain; charset=utf-8");
    }
}
