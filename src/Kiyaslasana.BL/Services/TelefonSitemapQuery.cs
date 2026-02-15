using Kiyaslasana.BL.Abstractions;
using Kiyaslasana.BL.Helpers;

namespace Kiyaslasana.BL.Services;

public sealed class TelefonSitemapQuery : ITelefonSitemapQuery
{
    private readonly ITelefonRepository _telefonRepository;

    public TelefonSitemapQuery(ITelefonRepository telefonRepository)
    {
        _telefonRepository = telefonRepository;
    }

    public Task<int> CountAsync(CancellationToken ct)
    {
        return _telefonRepository.CountAsync(ct);
    }

    public Task<IReadOnlyList<string>> GetSlugsPageAsync(int skip, int take, CancellationToken ct)
    {
        return _telefonRepository.GetSlugsPageAsync(skip, take, ct);
    }

    public async Task<IReadOnlyList<string>> GetBrandSlugsAsync(CancellationToken ct)
    {
        var brands = await _telefonRepository.GetDistinctBrandsAsync(ct);
        return brands
            .Select(BrandSlugHelper.ToSlug)
            .Where(x => x.Length > 0)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToArray();
    }
}
