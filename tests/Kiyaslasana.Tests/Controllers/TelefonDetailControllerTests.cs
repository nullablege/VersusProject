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

public class TelefonDetailControllerTests
{
    [Fact]
    public async Task Detail_WhenReviewHasSeoDescription_UsesReviewSeoForMetaAndJsonLd()
    {
        var controller = CreateController();

        var result = await controller.Detail("test-phone", CancellationToken.None);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<TelefonDetailViewModel>(view.Model);
        Assert.NotNull(model.Review);
        Assert.Equal("Review SEO Title", controller.ViewData["Title"]);
        Assert.Equal("Review SEO Description", controller.ViewData["MetaDescription"]);

        using var json = JsonDocument.Parse(model.ProductJsonLd);
        Assert.Equal("https://schema.org", json.RootElement.GetProperty("@context").GetString());
        Assert.Equal("Product", json.RootElement.GetProperty("@type").GetString());
        Assert.Equal("Phone", json.RootElement.GetProperty("name").GetString());
        Assert.Equal("Test", json.RootElement.GetProperty("brand").GetProperty("name").GetString());
        Assert.Equal("https://kiyaslasana.com/telefon/test-phone", json.RootElement.GetProperty("url").GetString());
        Assert.Equal("Review SEO Description", json.RootElement.GetProperty("description").GetString());
        Assert.Equal("test-phone", json.RootElement.GetProperty("sku").GetString());

        var review = json.RootElement.GetProperty("review");
        Assert.Equal("Review", review.GetProperty("@type").GetString());
        Assert.Equal("safe", review.GetProperty("reviewBody").GetString());
        Assert.False(string.IsNullOrWhiteSpace(review.GetProperty("datePublished").GetString()));
    }

    [Fact]
    public async Task Detail_WhenSeoDescriptionMissing_UsesExcerptForJsonLdDescription()
    {
        var controller = CreateController();

        var result = await controller.Detail("excerpt-phone", CancellationToken.None);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<TelefonDetailViewModel>(view.Model);
        using var json = JsonDocument.Parse(model.ProductJsonLd);

        Assert.Equal("Excerpt Description", json.RootElement.GetProperty("description").GetString());
    }

    [Fact]
    public async Task Detail_WhenSeoAndExcerptMissing_UsesDerivedReviewBodyForJsonLdDescription()
    {
        var controller = CreateController();

        var result = await controller.Detail("body-phone", CancellationToken.None);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<TelefonDetailViewModel>(view.Model);
        using var json = JsonDocument.Parse(model.ProductJsonLd);

        Assert.Equal("Derived Review Body & More", json.RootElement.GetProperty("description").GetString());
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
        private static readonly HashSet<string> KnownSlugs = ["test-phone", "excerpt-phone", "body-phone"];

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
            if (!KnownSlugs.Contains(slug))
            {
                return Task.FromResult<Telefon?>(null);
            }

            return Task.FromResult<Telefon?>(new Telefon
            {
                Slug = slug,
                Marka = "Test",
                ModelAdi = "Phone",
                Fiyat = "100"
            });
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
            if (slug == "test-phone")
            {
                return Task.FromResult<TelefonReview?>(new TelefonReview
                {
                    TelefonSlug = "test-phone",
                    Title = "Inceleme",
                    Excerpt = "Ozet",
                    RawContent = "<p>raw</p>",
                    SanitizedContent = "<p>safe</p>",
                    SeoTitle = "Review SEO Title",
                    SeoDescription = "Review SEO Description",
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                });
            }

            if (slug == "excerpt-phone")
            {
                return Task.FromResult<TelefonReview?>(new TelefonReview
                {
                    TelefonSlug = "excerpt-phone",
                    Title = "Inceleme",
                    Excerpt = "Excerpt Description",
                    RawContent = "<p>raw</p>",
                    SanitizedContent = "<p>safe</p>",
                    SeoTitle = "Review SEO Title",
                    SeoDescription = null,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                });
            }

            if (slug == "body-phone")
            {
                return Task.FromResult<TelefonReview?>(new TelefonReview
                {
                    TelefonSlug = "body-phone",
                    Title = "Inceleme",
                    Excerpt = null,
                    RawContent = "<p>raw</p>",
                    SanitizedContent = "<p>Derived <strong>Review</strong> Body &amp; More</p>",
                    SeoTitle = "Review SEO Title",
                    SeoDescription = null,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                });
            }

            return Task.FromResult<TelefonReview?>(null);
        }

        public Task<TelefonReviewUpsertResult> UpsertAsync(string slug, TelefonReviewUpsertInput input, CancellationToken ct)
        {
            return Task.FromResult(new TelefonReviewUpsertResult(false, null, null));
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
        public Task<Telefon?> GetBySlugAsync(string slug, CancellationToken ct)
        {
            return Task.FromResult<Telefon?>(null);
        }

        public Task<List<Telefon>> GetBySlugsAsync(IEnumerable<string> slugs, CancellationToken ct)
        {
            return Task.FromResult(new List<Telefon>());
        }

        public Task<IReadOnlyList<Telefon>> GetLatestAsync(int take, CancellationToken ct)
        {
            IReadOnlyList<Telefon> list = [];
            return Task.FromResult(list);
        }

        public Task<IReadOnlyList<Telefon>> GetRelatedCandidatesAsync(int take, CancellationToken ct)
        {
            IReadOnlyList<Telefon> list = [];
            return Task.FromResult(list);
        }

        public Task<int> CountAsync(CancellationToken ct)
        {
            return Task.FromResult(0);
        }

        public Task<IReadOnlyList<string>> GetSlugsPageAsync(int skip, int take, CancellationToken ct)
        {
            IReadOnlyList<string> list = [];
            return Task.FromResult(list);
        }

        public Task<(IReadOnlyList<Telefon> Items, int TotalCount)> GetPagedAsync(int skip, int take, CancellationToken ct)
        {
            return Task.FromResult(((IReadOnlyList<Telefon>)[], 0));
        }

        public Task<(IReadOnlyList<Telefon> Items, int TotalCount)> GetPagedByBrandAsync(string brand, int skip, int take, CancellationToken ct)
        {
            return Task.FromResult(((IReadOnlyList<Telefon>)[], 0));
        }

        public Task<(IReadOnlyList<Telefon> Items, int TotalCount)> GetPagedByPredicateAsync(
            Expression<Func<Telefon, bool>> predicate,
            int skip,
            int take,
            CancellationToken ct)
        {
            return Task.FromResult(((IReadOnlyList<Telefon>)[], 0));
        }

        public Task<IReadOnlyList<Telefon>> GetLatestByBrandAsync(string brand, int take, CancellationToken ct)
        {
            IReadOnlyList<Telefon> list = [];
            return Task.FromResult(list);
        }

        public Task<IReadOnlyList<string>> GetDistinctBrandsAsync(CancellationToken ct)
        {
            IReadOnlyList<string> list = [];
            return Task.FromResult(list);
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
