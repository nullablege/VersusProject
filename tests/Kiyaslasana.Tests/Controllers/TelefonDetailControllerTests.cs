using System.Linq.Expressions;
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
    public async Task Detail_WhenReviewExists_UsesReviewSeoAndSetsReviewModel()
    {
        var controller = new TelefonController(
            new StubTelefonService(),
            new StubTelefonReviewService(),
            new StubTelefonRepository(),
            new MemoryCache(new MemoryCacheOptions()));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = BuildHttpContext()
        };

        var result = await controller.Detail("test-phone", CancellationToken.None);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<TelefonDetailViewModel>(view.Model);
        Assert.NotNull(model.Review);
        Assert.Equal("Review SEO Title", controller.ViewData["Title"]);
        Assert.Equal("Review SEO Description", controller.ViewData["MetaDescription"]);
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
            if (slug != "test-phone")
            {
                return Task.FromResult<Telefon?>(null);
            }

            return Task.FromResult<Telefon?>(new Telefon
            {
                Slug = "test-phone",
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
            if (slug != "test-phone")
            {
                return Task.FromResult<TelefonReview?>(null);
            }

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
