using System.Text.Json;
using System.Security.Claims;
using Kiyaslasana.BL.Abstractions;
using Kiyaslasana.BL.Contracts;
using Kiyaslasana.EL.Entities;
using Kiyaslasana.PL.Controllers;
using Kiyaslasana.PL.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Kiyaslasana.Tests.Controllers;

public class KarsilastirControllerTests
{
    [Fact]
    public async Task Index_Guest_AllowsTwoSlots()
    {
        var controller = new KarsilastirController(new StubTelefonService());
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = BuildHttpContext()
        };

        var result = await controller.Index(first: null, CancellationToken.None);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<CompareBuilderViewModel>(viewResult.Model);
        Assert.Equal(2, model.MaxAllowed);
    }

    [Fact]
    public async Task Index_MemberRole_AllowsFourSlots()
    {
        var controller = new KarsilastirController(new StubTelefonService());
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = BuildHttpContext("Member")
        };

        var result = await controller.Index(first: null, CancellationToken.None);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<CompareBuilderViewModel>(viewResult.Model);
        Assert.Equal(4, model.MaxAllowed);
    }

    [Fact]
    public async Task CompareTwo_ProducesParsableJsonLd()
    {
        var controller = new KarsilastirController(new StubTelefonService());
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = BuildHttpContext()
        };

        var result = await controller.CompareTwo("alpha", "zeta", CancellationToken.None);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<CompareViewModel>(viewResult.Model);
        Assert.NotNull(model.BreadcrumbJsonLd);
        Assert.NotNull(model.ItemListJsonLd);

        using var breadcrumbJson = JsonDocument.Parse(model.BreadcrumbJsonLd!);
        using var itemListJson = JsonDocument.Parse(model.ItemListJsonLd!);

        Assert.Equal("BreadcrumbList", breadcrumbJson.RootElement.GetProperty("@type").GetString());
        Assert.Equal("ItemList", itemListJson.RootElement.GetProperty("@type").GetString());
        Assert.Equal(2, itemListJson.RootElement.GetProperty("itemListElement").GetArrayLength());
    }

    [Fact]
    public async Task CompareTwo_NonCanonicalOrder_RedirectsPermanently()
    {
        var controller = new KarsilastirController(new StubTelefonService());
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = BuildHttpContext()
        };

        var result = await controller.CompareTwo("zeta", "alpha", CancellationToken.None);

        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.True(redirect.Permanent);
        Assert.Equal("/karsilastir/alpha-vs-zeta", redirect.Url);
        Assert.Equal("public, max-age=600", controller.Response.Headers.CacheControl.ToString());
    }

    private static HttpContext BuildHttpContext(params string[] roles)
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
        if (roles.Length > 0)
        {
            var claims = new List<Claim> { new(ClaimTypes.Name, "test-user") };
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        }

        return httpContext;
    }

    private sealed class StubTelefonService : ITelefonService
    {
        public string NormalizeSlug(string? slug)
        {
            return (slug ?? string.Empty).Trim().ToLowerInvariant();
        }

        public CompareParseResult ParseCompareSlugs(string? slugs, bool isAuthenticated)
        {
            return new CompareParseResult(false, null, [], [], isAuthenticated ? 4 : 2);
        }

        public Task<CompareResolveResult> ResolveCompareAsync(IEnumerable<string> slugs, bool isAuthenticated, CancellationToken ct)
        {
            var canonical = slugs
                .Select(NormalizeSlug)
                .Where(x => x.Length > 0)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(x => x, StringComparer.Ordinal)
                .ToArray();

            var phones = canonical.Select(slug => new Telefon
            {
                Slug = slug,
                Marka = slug == "alpha" ? "Apple" : "Samsung",
                ModelAdi = slug == "alpha" ? "iPhone" : "Galaxy",
                ResimUrl = slug == "alpha" ? "/images/alpha.webp" : "https://cdn.example.com/zeta.webp"
            }).ToArray();

            return Task.FromResult(new CompareResolveResult(
                IsValid: canonical.Length >= 2,
                ErrorMessage: null,
                CanonicalSlugs: canonical,
                Phones: phones,
                MaxAllowed: isAuthenticated ? 4 : 2));
        }

        public Task<Telefon?> GetBySlugAsync(string slug, CancellationToken ct)
        {
            return Task.FromResult<Telefon?>(null);
        }

        public Task<IReadOnlyList<Telefon>> GetBySlugsAsync(IReadOnlyList<string> slugs, CancellationToken ct)
        {
            IReadOnlyList<Telefon> list = [];
            return Task.FromResult(list);
        }

        public Task<IReadOnlyList<Telefon>> GetLatestAsync(int take, CancellationToken ct)
        {
            IReadOnlyList<Telefon> list = [];
            return Task.FromResult(list);
        }

        public Task<IReadOnlyList<Telefon>> GetSimilarPhonesAsync(string slug, int take, CancellationToken ct)
        {
            IReadOnlyList<Telefon> list = [];
            return Task.FromResult(list);
        }

        public Task<IReadOnlyList<RelatedComparisonLink>> GetTopComparedLinksAsync(string slug, int take, CancellationToken ct)
        {
            IReadOnlyList<RelatedComparisonLink> list = [];
            return Task.FromResult(list);
        }

        public Task<IReadOnlyList<RelatedComparisonLink>> GetRelatedComparisonLinksAsync(
            IReadOnlyList<string> currentSlugs,
            int perSlug,
            int totalMax,
            CancellationToken ct)
        {
            IReadOnlyList<RelatedComparisonLink> links = [];
            return Task.FromResult(links);
        }
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
