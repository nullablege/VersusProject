using Kiyaslasana.BL.Abstractions;
using Kiyaslasana.BL.Contracts;
using Kiyaslasana.BL.Services;
using Kiyaslasana.EL.Entities;
using Xunit;

namespace Kiyaslasana.Tests.Services;

public class BlogPostServiceTests
{
    [Fact]
    public async Task CreateAsync_SanitizesRawHtml_AndBuildsUniqueSlug()
    {
        var repository = new InMemoryBlogPostRepository();
        var service = new BlogPostService(repository, new StubTelefonService());

        var first = await service.CreateAsync(new BlogPostUpsertInput(
            Title: "Test Blog",
            Excerpt: "Kisa ozet",
            ContentRaw: "<p>Merhaba</p><script>alert('x')</script>",
            MetaTitle: null,
            MetaDescription: null,
            IsPublished: true,
            PublishedAt: null),
            CancellationToken.None);

        var second = await service.CreateAsync(new BlogPostUpsertInput(
            Title: "Test Blog",
            Excerpt: null,
            ContentRaw: "<p>Ikinci yazi</p>",
            MetaTitle: null,
            MetaDescription: null,
            IsPublished: true,
            PublishedAt: null),
            CancellationToken.None);

        Assert.True(first.Success);
        Assert.NotNull(first.Post);
        Assert.DoesNotContain("<script", first.Post!.ContentSanitized, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("test-blog", first.Post.Slug);

        Assert.True(second.Success);
        Assert.NotNull(second.Post);
        Assert.Equal("test-blog-2", second.Post!.Slug);
    }

    private sealed class InMemoryBlogPostRepository : IBlogPostRepository
    {
        private readonly List<BlogPost> _posts = [];
        private int _nextId = 1;

        public Task<(IReadOnlyList<BlogPost> Items, int TotalCount)> GetAdminPagedAsync(int skip, int take, CancellationToken ct)
        {
            var safeSkip = Math.Max(skip, 0);
            var safeTake = Math.Max(take, 0);
            var ordered = _posts.OrderByDescending(x => x.UpdatedAt).ThenByDescending(x => x.Id).ToArray();
            return Task.FromResult(((IReadOnlyList<BlogPost>)ordered.Skip(safeSkip).Take(safeTake).ToArray(), _posts.Count));
        }

        public Task<BlogPost?> GetByIdAsync(int id, CancellationToken ct)
        {
            return Task.FromResult(_posts.FirstOrDefault(x => x.Id == id));
        }

        public Task<BlogPost?> GetBySlugAsync(string slug, bool publishedOnly, CancellationToken ct)
        {
            var post = _posts.FirstOrDefault(x => x.Slug == slug);
            return Task.FromResult(post);
        }

        public Task<(IReadOnlyList<BlogPost> Items, int TotalCount)> GetPublishedPagedAsync(int skip, int take, CancellationToken ct)
        {
            var safeSkip = Math.Max(skip, 0);
            var safeTake = Math.Max(take, 0);
            var now = DateTimeOffset.UtcNow;
            var published = _posts
                .Where(x => x.IsPublished && x.PublishedAt is not null && x.PublishedAt <= now)
                .OrderByDescending(x => x.PublishedAt)
                .ThenByDescending(x => x.Id)
                .ToArray();

            return Task.FromResult(((IReadOnlyList<BlogPost>)published.Skip(safeSkip).Take(safeTake).ToArray(), published.Length));
        }

        public Task<bool> SlugExistsAsync(string slug, int? excludeId, CancellationToken ct)
        {
            var exists = _posts.Any(x => x.Slug == slug && (!excludeId.HasValue || x.Id != excludeId.Value));
            return Task.FromResult(exists);
        }

        public Task AddAsync(BlogPost post, CancellationToken ct)
        {
            post.Id = _nextId++;
            _posts.Add(post);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(BlogPost post, CancellationToken ct)
        {
            var index = _posts.FindIndex(x => x.Id == post.Id);
            if (index >= 0)
            {
                _posts[index] = post;
            }

            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<BlogPost>> GetPublishedSitemapItemsAsync(CancellationToken ct)
        {
            IReadOnlyList<BlogPost> items = _posts
                .Where(x => x.IsPublished)
                .OrderByDescending(x => x.UpdatedAt)
                .ToArray();
            return Task.FromResult(items);
        }
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
}
