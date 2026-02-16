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

        public StubBlogPostService(bool returnNullBySlug = false)
        {
            _returnNullBySlug = returnNullBySlug;
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
            IReadOnlyList<BlogPost> items =
            [
                new BlogPost
                {
                    Id = 1,
                    Title = "Test Yazi",
                    Slug = "test-yazi",
                    Excerpt = "Test icerik",
                    ContentRaw = "<p>raw</p>",
                    ContentSanitized = "<p>safe</p>",
                    IsPublished = true,
                    PublishedAt = DateTimeOffset.UtcNow,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                }
            ];

            return Task.FromResult((items, items.Count));
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
