using Kiyaslasana.BL.Contracts;
using Kiyaslasana.EL.Entities;

namespace Kiyaslasana.BL.Abstractions;

public interface ITelefonReviewRepository
{
    Task<TelefonReview?> GetByTelefonSlugAsync(string telefonSlug, CancellationToken ct);

    Task<(IReadOnlyList<TelefonReviewAdminListItem> Items, int TotalCount)> GetAdminPagedAsync(
        string? searchTerm,
        int skip,
        int take,
        CancellationToken ct);

    Task AddAsync(TelefonReview review, CancellationToken ct);

    Task UpdateAsync(TelefonReview review, CancellationToken ct);

    Task DeleteAsync(TelefonReview review, CancellationToken ct);
}
