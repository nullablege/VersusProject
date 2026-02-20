using Kiyaslasana.PL.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Kiyaslasana.Tests.Controllers;

public class RobotsControllerTests
{
    [Fact]
    public void Index_ReturnsExpectedRules_AndUsesConfiguredSitemapBaseUrl()
    {
        var controller = new RobotsController
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = BuildHttpContext()
            }
        };

        var result = controller.Index();

        var content = Assert.IsType<ContentResult>(result);
        Assert.Equal("text/plain; charset=utf-8", content.ContentType);
        Assert.NotNull(content.Content);
        Assert.Contains("User-agent: *", content.Content, StringComparison.Ordinal);
        Assert.Contains("Disallow: /*?*sort=", content.Content, StringComparison.Ordinal);
        Assert.Contains("Disallow: /telefonlar/marka/*?*page=", content.Content, StringComparison.Ordinal);
        Assert.Contains("Disallow: /telefonlar/*?*page=", content.Content, StringComparison.Ordinal);
        Assert.Contains("Sitemap: https://kiyaslasana.com/sitemap.xml", content.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("spoofed.invalid", content.Content, StringComparison.Ordinal);
    }

    private static HttpContext BuildHttpContext()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Seo:PublicBaseUrl"] = "https://kiyaslasana.com"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddControllersWithViews();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton<IHostEnvironment>(new TestHostEnvironment(Environments.Production));

        var httpContext = new DefaultHttpContext
        {
            RequestServices = services.BuildServiceProvider()
        };

        httpContext.Request.Scheme = "http";
        httpContext.Request.Host = new HostString("spoofed.invalid");
        return httpContext;
    }

    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public TestHostEnvironment(string environmentName)
        {
            EnvironmentName = environmentName;
        }

        public string EnvironmentName { get; set; }

        public string ApplicationName { get; set; } = "Kiyaslasana.Tests";

        public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();

        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } =
            new Microsoft.Extensions.FileProviders.NullFileProvider();
    }
}
