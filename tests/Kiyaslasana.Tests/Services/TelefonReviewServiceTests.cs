using Kiyaslasana.BL.Abstractions;
using Kiyaslasana.BL.Contracts;
using Kiyaslasana.BL.Services;
using Kiyaslasana.EL.Entities;

namespace Kiyaslasana.Tests.Services;

public class TelefonReviewServiceTests
{
    [Fact]
    public async Task UpsertAndGet_RemovesScriptTagsFromSanitizedContent()
    {
        var repository = new InMemoryTelefonReviewRepository();
        var service = new TelefonReviewService(
            repository,
            new StubTelefonRepository(phoneExists: true),
            new StubTelefonService());

        var upsert = await service.UpsertAsync("test-phone", new TelefonReviewUpsertInput(
            Title: "Detayli Inceleme",
            Excerpt: "Kisa ozet",
            RawContent: "<p>Guvenli</p><script>alert('x')</script>",
            SeoTitle: "SEO Baslik",
            SeoDescription: "SEO Aciklama"),
            CancellationToken.None);

        var review = await service.GetByTelefonSlugAsync("test-phone", CancellationToken.None);

        Assert.True(upsert.Success);
        Assert.NotNull(review);
        Assert.Contains("<p>Guvenli</p>", review!.SanitizedContent, StringComparison.Ordinal);
        Assert.DoesNotContain("<script", review.SanitizedContent, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Upsert_ReturnsError_WhenTelefonNotFound()
    {
        var service = new TelefonReviewService(
            new InMemoryTelefonReviewRepository(),
            new StubTelefonRepository(phoneExists: false),
            new StubTelefonService());

        var result = await service.UpsertAsync("missing-phone", new TelefonReviewUpsertInput(
            Title: null,
            Excerpt: null,
            RawContent: "<p>Test</p>",
            SeoTitle: null,
            SeoDescription: null),
            CancellationToken.None);

        Assert.False(result.Success);
        Assert.Contains("telefon", result.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class InMemoryTelefonReviewRepository : ITelefonReviewRepository
    {
        private readonly Dictionary<string, TelefonReview> _reviews = new(StringComparer.Ordinal);

        public Task<TelefonReview?> GetByTelefonSlugAsync(string telefonSlug, CancellationToken ct)
        {
            _reviews.TryGetValue(telefonSlug, out var review);
            return Task.FromResult(review);
        }

        public Task<(IReadOnlyList<TelefonReviewAdminListItem> Items, int TotalCount)> GetAdminPagedAsync(
            string? searchTerm,
            int skip,
            int take,
            CancellationToken ct)
        {
            IReadOnlyList<TelefonReviewAdminListItem> items = [];
            return Task.FromResult((items, 0));
        }

        public Task AddAsync(TelefonReview review, CancellationToken ct)
        {
            review.Id = _reviews.Count + 1;
            _reviews[review.TelefonSlug] = review;
            return Task.CompletedTask;
        }

        public Task UpdateAsync(TelefonReview review, CancellationToken ct)
        {
            _reviews[review.TelefonSlug] = review;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(TelefonReview review, CancellationToken ct)
        {
            _reviews.Remove(review.TelefonSlug);
            return Task.CompletedTask;
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

        public Task<IReadOnlyList<Telefon>> GetSimilarPhonesAsync(string slug, int take, CancellationToken ct)
        {
            IReadOnlyList<Telefon> list = [];
            return Task.FromResult(list);
        }

        public Task RecordCompareVisitAsync(string slugLeft, string slugRight, string? ipHash, CancellationToken ct)
        {
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<TopComparedPair>> GetTopComparedAsync(int take, CancellationToken ct)
        {
            IReadOnlyList<TopComparedPair> list = [];
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
            IReadOnlyList<RelatedComparisonLink> list = [];
            return Task.FromResult(list);
        }
    }

    private sealed class StubTelefonRepository : ITelefonRepository
    {
        private readonly bool _phoneExists;

        public StubTelefonRepository(bool phoneExists)
        {
            _phoneExists = phoneExists;
        }

        public Task<Telefon?> GetBySlugAsync(string slug, CancellationToken ct)
        {
            if (!_phoneExists)
            {
                return Task.FromResult<Telefon?>(null);
            }

            return Task.FromResult<Telefon?>(new Telefon
            {
                Slug = slug,
                Marka = "Test",
                ModelAdi = "Model"
            });
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
            System.Linq.Expressions.Expression<Func<Telefon, bool>> predicate,
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

        public Task<IReadOnlyList<Telefon>> GetDetailSimilarAsync(string? brand, string excludeSlug, int take, CancellationToken ct)
        {
            IReadOnlyList<Telefon> list = [];
            return Task.FromResult(list);
        }
    }
}
