using System.Text.Json;
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

public class BlogControllerTests
{
    [Fact]
    public async Task Detail_ReturnsValidBlogPostingJsonLd()
    {
        var controller = CreateController();

        var result = await controller.Detail("test-yazi", CancellationToken.None);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<BlogDetailViewModel>(view.Model);
        Assert.Contains("<a href=\"/telefon/test-phone\">", model.ContentHtml, StringComparison.Ordinal);

        using var doc = JsonDocument.Parse(model.BlogPostingJsonLd);
        Assert.Equal("BlogPosting", doc.RootElement.GetProperty("@type").GetString());
        Assert.Equal("Test Yazi", doc.RootElement.GetProperty("headline").GetString());
        Assert.Equal("https://kiyaslasana.com/blog/test-yazi", doc.RootElement.GetProperty("url").GetString());
        Assert.Equal("https://kiyaslasana.com/blog/test-yazi", controller.ViewData["Canonical"]);
    }

    [Fact]
    public async Task Detail_MissingPost_ReturnsNotFound()
    {
        var controller = CreateController(new StubBlogPostService(returnNullBySlug: true));

        var result = await controller.Detail("olmayan", CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Index_Page2_SetsSelfCanonicalAndPrevNextLinks()
    {
        var controller = CreateController(new StubBlogPostService(publishedTotalCount: 40));

        var result = await controller.Index(page: 2, CancellationToken.None);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<BlogListViewModel>(view.Model);
        Assert.Equal(2, model.Page);
        Assert.Equal("https://kiyaslasana.com/blog?page=2", controller.ViewData["Canonical"]);
        Assert.Equal("https://kiyaslasana.com/blog", controller.ViewData["PrevUrl"]);
        Assert.Equal("https://kiyaslasana.com/blog?page=3", controller.ViewData["NextUrl"]);
        Assert.Equal("index,follow", controller.ViewData["Robots"]);
    }

    [Fact]
    public async Task Index_PageBeyondTotal_RedirectsToLastPage()
    {
        var controller = CreateController(new StubBlogPostService(publishedTotalCount: 30));

        var result = await controller.Index(page: 99, CancellationToken.None);

        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.True(redirect.Permanent);
        Assert.Equal("/blog?page=3", redirect.Url);
    }

    private static BlogController CreateController(IBlogPostService? blogPostService = null)
    {
        var controller = new BlogController(blogPostService ?? new StubBlogPostService());
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

    private sealed class StubBlogPostService : IBlogPostService
    {
        private readonly bool _returnNullBySlug;
        private readonly int _publishedTotalCount;

        public StubBlogPostService(bool returnNullBySlug = false, int publishedTotalCount = 1)
        {
            _returnNullBySlug = returnNullBySlug;
            _publishedTotalCount = Math.Max(publishedTotalCount, 0);
        }

        public Task<(IReadOnlyList<BlogPost> Items, int TotalCount)> GetAdminPagedAsync(int page, int pageSize, CancellationToken ct)
        {
            IReadOnlyList<BlogPost> items = [];
            return Task.FromResult((items, 0));
        }

        public Task<BlogPost?> GetAdminByIdAsync(int id, CancellationToken ct)
        {
            return Task.FromResult<BlogPost?>(null);
        }

        public Task<BlogPostUpsertResult> CreateAsync(BlogPostUpsertInput input, CancellationToken ct)
        {
            return Task.FromResult(new BlogPostUpsertResult(false, "Not implemented in test.", null));
        }

        public Task<BlogPostUpsertResult> UpdateAsync(int id, BlogPostUpsertInput input, CancellationToken ct)
        {
            return Task.FromResult(new BlogPostUpsertResult(false, "Not implemented in test.", null));
        }

        public Task<(IReadOnlyList<BlogPost> Items, int TotalCount)> GetPublishedPagedAsync(int page, int pageSize, CancellationToken ct)
        {
            var safePageSize = Math.Max(pageSize, 1);
            var safePage = Math.Max(page, 1);
            var totalPages = Math.Max(1, (int)Math.Ceiling(_publishedTotalCount / (double)safePageSize));
            var effectivePage = Math.Min(safePage, totalPages);
            var skip = (effectivePage - 1) * safePageSize;
            var remaining = Math.Max(_publishedTotalCount - skip, 0);
            var take = Math.Min(safePageSize, remaining);
            var now = DateTimeOffset.UtcNow;

            var items = Enumerable.Range(0, take)
                .Select(index => new BlogPost
                {
                    Id = skip + index + 1,
                    Title = $"Test Yazi {skip + index + 1}",
                    Slug = $"test-yazi-{skip + index + 1}",
                    Excerpt = "Test icerik",
                    ContentRaw = "<p>raw</p>",
                    ContentSanitized = "<p>safe</p>",
                    IsPublished = true,
                    PublishedAt = now,
                    CreatedAt = now,
                    UpdatedAt = now
                })
                .ToArray();

            return Task.FromResult(((IReadOnlyList<BlogPost>)items, _publishedTotalCount));
        }

        public Task<BlogPost?> GetPublishedBySlugAsync(string slug, CancellationToken ct)
        {
            if (_returnNullBySlug)
            {
                return Task.FromResult<BlogPost?>(null);
            }

            return Task.FromResult<BlogPost?>(new BlogPost
            {
                Id = 1,
                Title = "Test Yazi",
                Slug = "test-yazi",
                Excerpt = "Test ozet",
                ContentRaw = "<p>raw</p>",
                ContentSanitized = "<p>safe</p>",
                MetaTitle = "Test Meta Title",
                MetaDescription = "Test Meta Description",
                IsPublished = true,
                PublishedAt = DateTimeOffset.Parse("2026-01-01T00:00:00Z"),
                CreatedAt = DateTimeOffset.Parse("2025-12-31T00:00:00Z"),
                UpdatedAt = DateTimeOffset.Parse("2026-01-02T00:00:00Z")
            });
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
            return Task.FromResult("<p>safe <a href=\"/telefon/test-phone\">test-phone</a></p>");
        }

        public Task<IReadOnlyList<BlogPost>> GetLatestPublishedMentioningTelefonSlugAsync(string telefonSlug, int take, CancellationToken ct)
        {
            IReadOnlyList<BlogPost> posts = [];
            return Task.FromResult(posts);
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
