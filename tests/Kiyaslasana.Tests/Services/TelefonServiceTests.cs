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

        public Task<IReadOnlyList<Telefon>> GetBySlugsAsync(IReadOnlyList<string> slugs, CancellationToken ct)
        {
            IReadOnlyList<Telefon> list = slugs.Select(x => new Telefon { Slug = x, ModelAdi = x }).ToArray();
            return Task.FromResult(list);
        }

        public Task<IReadOnlyList<Telefon>> GetLatestAsync(int take, CancellationToken ct)
        {
            IReadOnlyList<Telefon> list = Array.Empty<Telefon>();
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
    }
}
