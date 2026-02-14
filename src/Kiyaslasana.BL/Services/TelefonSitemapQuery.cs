using Kiyaslasana.BL.Abstractions;

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
}
