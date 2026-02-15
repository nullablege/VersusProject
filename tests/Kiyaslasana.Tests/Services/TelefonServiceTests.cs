using Kiyaslasana.BL.Abstractions;
using Kiyaslasana.BL.Services;
using Kiyaslasana.EL.Entities;
using Microsoft.Extensions.Caching.Memory;

namespace Kiyaslasana.Tests.Services;

public class TelefonServiceTests
{
    [Fact]
    public void NormalizeSlug_TrimsAndLowercases()
    {
        var service = CreateService();

        var normalized = service.NormalizeSlug("  Galaxy-S24-Ultra  ");

        Assert.Equal("galaxy-s24-ultra", normalized);
    }

    [Fact]
    public void ParseCompareSlugs_RemovesDuplicates_PreservesInput_AndBuildsCanonical()
    {
        var service = CreateService();

        var result = service.ParseCompareSlugs("zeta-vs-alpha-vs-zeta-vs-beta", isAuthenticated: true);

        Assert.True(result.IsValid);
        Assert.Equal(new[] { "zeta", "alpha", "beta" }, result.RequestedSlugs);
        Assert.Equal(new[] { "alpha", "beta", "zeta" }, result.CanonicalSlugs);
    }

    [Fact]
    public void ParseCompareSlugs_Guest_RejectsMoreThanTwo()
    {
        var service = CreateService();

        var result = service.ParseCompareSlugs("a-vs-b-vs-c", isAuthenticated: false);

        Assert.False(result.IsValid);
        Assert.Equal(2, result.MaxAllowed);
        Assert.Contains("En fazla 2", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ParseCompareSlugs_Member_AllowsUpToFour()
    {
        var service = CreateService();

        var result = service.ParseCompareSlugs("d-vs-c-vs-b-vs-a", isAuthenticated: true);

        Assert.True(result.IsValid);
        Assert.Equal(4, result.MaxAllowed);
        Assert.Equal(4, result.CanonicalSlugs.Count);
    }

    [Fact]
    public void ParseCompareSlugs_RequiresAtLeastTwo()
    {
        var service = CreateService();

        var result = service.ParseCompareSlugs("just-one", isAuthenticated: true);

        Assert.False(result.IsValid);
        Assert.Contains("en az 2", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ResolveCompareAsync_ReturnsCanonicalOrderedPhones()
    {
        var service = CreateService();

        var result = await service.ResolveCompareAsync(["zeta", "alpha"], isAuthenticated: true, CancellationToken.None);

        Assert.True(result.IsValid);
        Assert.Equal(new[] { "alpha", "zeta" }, result.CanonicalSlugs);
        Assert.Equal(new[] { "alpha", "zeta" }, result.Phones.Select(x => x.Slug));
    }

    [Fact]
    public async Task GetRelatedComparisonLinksAsync_CreatesCanonicalUniquePairs()
    {
        var service = CreateService();

        var links = await service.GetRelatedComparisonLinksAsync(["alpha", "beta"], perSlug: 6, totalMax: 12, CancellationToken.None);

        Assert.NotEmpty(links);
        Assert.All(links, link => Assert.True(string.Compare(link.CanonicalLeftSlug, link.CanonicalRightSlug, StringComparison.Ordinal) < 0));
        Assert.Equal(links.Count, links.Select(x => $"{x.CanonicalLeftSlug}|{x.CanonicalRightSlug}").Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public async Task GetRelatedComparisonLinksAsync_RespectsPerSlugAndTotalLimits()
    {
        var service = CreateService();

        var links = await service.GetRelatedComparisonLinksAsync(["alpha"], perSlug: 2, totalMax: 3, CancellationToken.None);

        Assert.Equal(2, links.Count);
    }

    private static TelefonService CreateService()
    {
        return new TelefonService(new StubTelefonRepository(), new MemoryCache(new MemoryCacheOptions()));
    }

    private sealed class StubTelefonRepository : ITelefonRepository
    {
        public Task<Telefon?> GetBySlugAsync(string slug, CancellationToken ct)
        {
            return Task.FromResult<Telefon?>(new Telefon { Slug = slug, ModelAdi = slug });
        }

        public Task<List<Telefon>> GetBySlugsAsync(IEnumerable<string> slugs, CancellationToken ct)
        {
            var list = slugs.Select(x => new Telefon { Slug = x, ModelAdi = x }).ToList();
            return Task.FromResult(list);
        }

        public Task<IReadOnlyList<Telefon>> GetLatestAsync(int take, CancellationToken ct)
        {
            IReadOnlyList<Telefon> list = Array.Empty<Telefon>();
            return Task.FromResult(list);
        }

        public Task<IReadOnlyList<Telefon>> GetRelatedCandidatesAsync(int take, CancellationToken ct)
        {
            IReadOnlyList<Telefon> list =
            [
                new Telefon { Slug = "gamma", Marka = "Brand", ModelAdi = "Gamma" },
                new Telefon { Slug = "delta", Marka = "Brand", ModelAdi = "Delta" },
                new Telefon { Slug = "epsilon", Marka = "Brand", ModelAdi = "Epsilon" },
                new Telefon { Slug = "zeta", Marka = "Brand", ModelAdi = "Zeta" }
            ];
            return Task.FromResult(list);
        }

        public Task<int> CountAsync(CancellationToken ct)
        {
            return Task.FromResult(0);
        }

        public Task<IReadOnlyList<string>> GetSlugsPageAsync(int skip, int take, CancellationToken ct)
        {
            IReadOnlyList<string> list = Array.Empty<string>();
            return Task.FromResult(list);
        }

        public Task<(IReadOnlyList<Telefon> Items, int TotalCount)> GetPagedAsync(int skip, int take, CancellationToken ct)
        {
            return Task.FromResult(((IReadOnlyList<Telefon>)Array.Empty<Telefon>(), 0));
        }

        public Task<(IReadOnlyList<Telefon> Items, int TotalCount)> GetPagedByBrandAsync(string brand, int skip, int take, CancellationToken ct)
        {
            return Task.FromResult(((IReadOnlyList<Telefon>)Array.Empty<Telefon>(), 0));
        }

        public Task<(IReadOnlyList<Telefon> Items, int TotalCount)> GetPagedByPredicateAsync(
            System.Linq.Expressions.Expression<Func<Telefon, bool>> predicate,
            int skip,
            int take,
            CancellationToken ct)
        {
            return Task.FromResult(((IReadOnlyList<Telefon>)Array.Empty<Telefon>(), 0));
        }

        public Task<IReadOnlyList<Telefon>> GetLatestByBrandAsync(string brand, int take, CancellationToken ct)
        {
            IReadOnlyList<Telefon> list = Array.Empty<Telefon>();
            return Task.FromResult(list);
        }

        public Task<IReadOnlyList<string>> GetDistinctBrandsAsync(CancellationToken ct)
        {
            IReadOnlyList<string> list = Array.Empty<string>();
            return Task.FromResult(list);
        }
    }
}
