using Kiyaslasana.BL.Abstractions;
using Kiyaslasana.BL.Services;
using Kiyaslasana.EL.Entities;

namespace Kiyaslasana.Tests.Services;

public class TelefonSitemapQueryTests
{
    [Fact]
    public async Task GetBrandSlugsAsync_ReturnsNonEmpty_WhenBrandsExist()
    {
        var query = new TelefonSitemapQuery(new StubTelefonRepository());

        var brandSlugs = await query.GetBrandSlugsAsync(CancellationToken.None);

        Assert.NotEmpty(brandSlugs);
        Assert.Contains("apple", brandSlugs);
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
            IReadOnlyList<string> list = ["Apple", "Samsung"];
            return Task.FromResult(list);
        }

        public Task<IReadOnlyList<Telefon>> GetDetailSimilarAsync(string? brand, string excludeSlug, int take, CancellationToken ct)
        {
            IReadOnlyList<Telefon> list = [];
            return Task.FromResult(list);
        }
    }
}
