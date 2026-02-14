namespace Kiyaslasana.BL.Abstractions;

public interface ITelefonSitemapQuery
{
    Task<int> CountAsync(CancellationToken ct);

    Task<IReadOnlyList<string>> GetSlugsPageAsync(int skip, int take, CancellationToken ct);
}
