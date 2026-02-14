using Kiyaslasana.EL.Entities;

namespace Kiyaslasana.BL.Abstractions;

public interface ITelefonRepository
{
    Task<Telefon?> GetBySlugAsync(string slug, CancellationToken ct);

    Task<IReadOnlyList<Telefon>> GetBySlugsAsync(IReadOnlyList<string> slugs, CancellationToken ct);

    Task<IReadOnlyList<Telefon>> GetLatestAsync(int take, CancellationToken ct);

    Task<int> CountAsync(CancellationToken ct);

    Task<IReadOnlyList<string>> GetSlugsPageAsync(int skip, int take, CancellationToken ct);
}
