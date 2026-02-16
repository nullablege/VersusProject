using System.Linq.Expressions;
using System.Text.Json;
using Kiyaslasana.BL.Abstractions;
using Kiyaslasana.BL.Contracts;
using Kiyaslasana.EL.Entities;
using Kiyaslasana.PL.Controllers;
using Kiyaslasana.PL.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Kiyaslasana.Tests.Controllers;

public class TelefonFilterControllerTests
{
    [Fact]
    public async Task ByFilter_ValidSlug_ReturnsViewResult()
    {
        var controller = CreateController();

        var result = await controller.ByFilter("5g-telefonlar", page: 1, CancellationToken.None);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", view.ViewName);
        Assert.IsType<TelefonListViewModel>(view.Model);
    }

    [Fact]
    public async Task ByFilter_InvalidSlug_ReturnsNotFound()
    {
        var controller = CreateController();

        var result = await controller.ByFilter("olmayan-filtre", page: 1, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task ByFilter_Page2_SetsNoindex()
    {
        var controller = CreateController();

        var result = await controller.ByFilter("5g-telefonlar", page: 2, CancellationToken.None);

        Assert.IsType<ViewResult>(result);
        Assert.Equal("noindex,follow", controller.ViewData["Robots"]);
    }

    [Fact]
    public async Task ByFilter_Canonical_IsCorrectForPage1AndPage2()
    {
        var page1Controller = CreateController();
        var page1Result = await page1Controller.ByFilter("5g-telefonlar", page: 1, CancellationToken.None);
        Assert.IsType<ViewResult>(page1Result);

        var page2Controller = CreateController();
        var page2Result = await page2Controller.ByFilter("5g-telefonlar", page: 2, CancellationToken.None);
        Assert.IsType<ViewResult>(page2Result);

        Assert.Equal("https://kiyaslasana.com/telefonlar/5g-telefonlar", page1Controller.ViewData["Canonical"]);
        Assert.Equal("https://kiyaslasana.com/telefonlar/5g-telefonlar?page=2", page2Controller.ViewData["Canonical"]);
    }

    [Fact]
    public async Task ByFilter_SetsValidItemListJsonLd()
    {
        var controller = CreateController();
        var result = await controller.ByFilter("5g-telefonlar", page: 1, CancellationToken.None);

        Assert.IsType<ViewResult>(result);
        var jsonLd = Assert.IsType<string>(controller.ViewData["FilterItemListJsonLd"]);

        using var doc = JsonDocument.Parse(jsonLd);
        Assert.Equal("https://schema.org", doc.RootElement.GetProperty("@context").GetString());
        Assert.Equal("ItemList", doc.RootElement.GetProperty("@type").GetString());

        var items = doc.RootElement.GetProperty("itemListElement");
        Assert.True(items.GetArrayLength() > 0);

        var first = items[0];
        Assert.Equal("Product", first.GetProperty("@type").GetString());
        Assert.Equal(1, first.GetProperty("position").GetInt32());
        Assert.False(string.IsNullOrWhiteSpace(first.GetProperty("url").GetString()));
    }

    private static TelefonController CreateController()
    {
        var controller = new TelefonController(
            new StubTelefonService(),
            new StubTelefonReviewService(),
            new StubBlogPostService(),
            new StubTelefonRepository(),
            new MemoryCache(new MemoryCacheOptions()));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = BuildHttpContext()
        };

        return controller;
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
            return Task.FromResult(new CompareResolveResult(false, null, [], [], isAuthenticated ? 4 : 2));
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

        public Task<IReadOnlyList<RelatedComparisonLink>> GetRelatedComparisonLinksAsync(
            IReadOnlyList<string> currentSlugs,
            int perSlug,
            int totalMax,
            CancellationToken ct)
        {
            IReadOnlyList<RelatedComparisonLink> list = [];
            return Task.FromResult(list);
        }
    }

    private sealed class StubTelefonReviewService : ITelefonReviewService
    {
        public Task<TelefonReview?> GetByTelefonSlugAsync(string slug, CancellationToken ct)
        {
            return Task.FromResult<TelefonReview?>(null);
        }

        public Task<TelefonReviewUpsertResult> UpsertAsync(string slug, TelefonReviewUpsertInput input, CancellationToken ct)
        {
            return Task.FromResult(new TelefonReviewUpsertResult(false, "Not implemented for test.", null));
        }

        public Task<bool> DeleteAsync(string slug, CancellationToken ct)
        {
            return Task.FromResult(false);
        }

        public Task<(IReadOnlyList<TelefonReviewAdminListItem> Items, int TotalCount)> GetAdminPagedAsync(
            string? query,
            int page,
            int pageSize,
            CancellationToken ct)
        {
            return Task.FromResult(((IReadOnlyList<TelefonReviewAdminListItem>)[], 0));
        }

        public Task<IReadOnlyList<Telefon>> GetAdminPhoneSuggestionsAsync(int take, CancellationToken ct)
        {
            IReadOnlyList<Telefon> list = [];
            return Task.FromResult(list);
        }
    }

    private sealed class StubTelefonRepository : ITelefonRepository
    {
        private static readonly IReadOnlyList<Telefon> Phones = Enumerable.Range(1, 60)
            .Select(x => new Telefon
            {
                Slug = $"test-5g-{x:D3}",
                Marka = "TestBrand",
                ModelAdi = $"Model {x}",
                NetworkTeknolojisi = "GSM / HSPA / LTE / 5G"
            })
            .ToArray();

        public Task<Telefon?> GetBySlugAsync(string slug, CancellationToken ct)
        {
            return Task.FromResult(Phones.FirstOrDefault(x => x.Slug == slug));
        }

        public Task<List<Telefon>> GetBySlugsAsync(IEnumerable<string> slugs, CancellationToken ct)
        {
            var slugSet = slugs.ToHashSet(StringComparer.Ordinal);
            return Task.FromResult(Phones.Where(x => x.Slug is not null && slugSet.Contains(x.Slug)).ToList());
        }

        public Task<IReadOnlyList<Telefon>> GetLatestAsync(int take, CancellationToken ct)
        {
            return Task.FromResult((IReadOnlyList<Telefon>)Phones.Take(Math.Clamp(take, 1, 50)).ToArray());
        }

        public Task<IReadOnlyList<Telefon>> GetRelatedCandidatesAsync(int take, CancellationToken ct)
        {
            return Task.FromResult((IReadOnlyList<Telefon>)Phones.Take(Math.Clamp(take, 1, 500)).ToArray());
        }

        public Task<int> CountAsync(CancellationToken ct)
        {
            return Task.FromResult(Phones.Count);
        }

        public Task<IReadOnlyList<string>> GetSlugsPageAsync(int skip, int take, CancellationToken ct)
        {
            var safeSkip = Math.Max(skip, 0);
            var safeTake = Math.Max(take, 0);
            return Task.FromResult((IReadOnlyList<string>)Phones
                .Where(x => !string.IsNullOrWhiteSpace(x.Slug))
                .Select(x => x.Slug!)
                .OrderBy(x => x, StringComparer.Ordinal)
                .Skip(safeSkip)
                .Take(safeTake)
                .ToArray());
        }

        public Task<(IReadOnlyList<Telefon> Items, int TotalCount)> GetPagedAsync(int skip, int take, CancellationToken ct)
        {
            var ordered = Phones.OrderBy(x => x.Slug, StringComparer.Ordinal).ToArray();
            var safeSkip = Math.Max(skip, 0);
            var safeTake = Math.Max(take, 0);
            var items = ordered.Skip(safeSkip).Take(safeTake).ToArray();
            return Task.FromResult(((IReadOnlyList<Telefon>)items, ordered.Length));
        }

        public Task<(IReadOnlyList<Telefon> Items, int TotalCount)> GetPagedByBrandAsync(string brand, int skip, int take, CancellationToken ct)
        {
            var filtered = Phones.Where(x => x.Marka == brand).OrderBy(x => x.Slug, StringComparer.Ordinal).ToArray();
            var safeSkip = Math.Max(skip, 0);
            var safeTake = Math.Max(take, 0);
            var items = filtered.Skip(safeSkip).Take(safeTake).ToArray();
            return Task.FromResult(((IReadOnlyList<Telefon>)items, filtered.Length));
        }

        public Task<(IReadOnlyList<Telefon> Items, int TotalCount)> GetPagedByPredicateAsync(
            Expression<Func<Telefon, bool>> predicate,
            int skip,
            int take,
            CancellationToken ct)
        {
            var compiled = predicate.Compile();
            var filtered = Phones
                .Where(x => !string.IsNullOrWhiteSpace(x.Slug))
                .Where(compiled)
                .OrderBy(x => x.Slug, StringComparer.Ordinal)
                .ToArray();

            var safeTake = Math.Max(take, 0);
            var safeSkip = Math.Max(skip, 0);
            var maxSkip = filtered.Length <= 0 || safeTake <= 0 ? 0 : ((filtered.Length - 1) / safeTake) * safeTake;
            var effectiveSkip = Math.Min(safeSkip, maxSkip);
            var items = safeTake <= 0 ? [] : filtered.Skip(effectiveSkip).Take(safeTake).ToArray();

            return Task.FromResult(((IReadOnlyList<Telefon>)items, filtered.Length));
        }

        public Task<IReadOnlyList<Telefon>> GetLatestByBrandAsync(string brand, int take, CancellationToken ct)
        {
            var filtered = Phones.Where(x => x.Marka == brand).Take(Math.Clamp(take, 1, 50)).ToArray();
            return Task.FromResult((IReadOnlyList<Telefon>)filtered);
        }

        public Task<IReadOnlyList<string>> GetDistinctBrandsAsync(CancellationToken ct)
        {
            IReadOnlyList<string> brands = ["TestBrand"];
            return Task.FromResult(brands);
        }
    }

    private sealed class StubBlogPostService : IBlogPostService
    {
        public Task<(IReadOnlyList<BlogPost> Items, int TotalCount)> GetAdminPagedAsync(int page, int pageSize, CancellationToken ct)
        {
            return Task.FromResult(((IReadOnlyList<BlogPost>)[], 0));
        }

        public Task<BlogPost?> GetAdminByIdAsync(int id, CancellationToken ct)
        {
            return Task.FromResult<BlogPost?>(null);
        }

        public Task<BlogPostUpsertResult> CreateAsync(BlogPostUpsertInput input, CancellationToken ct)
        {
            return Task.FromResult(new BlogPostUpsertResult(false, null, null));
        }

        public Task<BlogPostUpsertResult> UpdateAsync(int id, BlogPostUpsertInput input, CancellationToken ct)
        {
            return Task.FromResult(new BlogPostUpsertResult(false, null, null));
        }

        public Task<(IReadOnlyList<BlogPost> Items, int TotalCount)> GetPublishedPagedAsync(int page, int pageSize, CancellationToken ct)
        {
            return Task.FromResult(((IReadOnlyList<BlogPost>)[], 0));
        }

        public Task<BlogPost?> GetPublishedBySlugAsync(string slug, CancellationToken ct)
        {
            return Task.FromResult<BlogPost?>(null);
        }

        public Task<IReadOnlyList<BlogPost>> GetPublishedSitemapItemsAsync(CancellationToken ct)
        {
            IReadOnlyList<BlogPost> items = [];
            return Task.FromResult(items);
        }

        public Task<IReadOnlyList<BlogInternalLink>> BuildInternalLinksAsync(BlogPost post, CancellationToken ct)
        {
            IReadOnlyList<BlogInternalLink> links = [];
            return Task.FromResult(links);
        }

        public Task<string> BuildTelefonSlugLinksAsync(string sanitizedHtml, CancellationToken ct)
        {
            return Task.FromResult(sanitizedHtml);
        }

        public Task<IReadOnlyList<BlogPost>> GetLatestPublishedMentioningTelefonSlugAsync(string telefonSlug, int take, CancellationToken ct)
        {
            IReadOnlyList<BlogPost> items = [];
            return Task.FromResult(items);
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
