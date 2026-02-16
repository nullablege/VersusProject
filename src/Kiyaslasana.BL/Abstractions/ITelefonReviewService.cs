using Kiyaslasana.BL.Contracts;
using Kiyaslasana.EL.Entities;

namespace Kiyaslasana.BL.Abstractions;

public interface ITelefonReviewService
{
    Task<TelefonReview?> GetByTelefonSlugAsync(string slug, CancellationToken ct);

    Task<TelefonReviewUpsertResult> UpsertAsync(string slug, TelefonReviewUpsertInput input, CancellationToken ct);

    Task<bool> DeleteAsync(string slug, CancellationToken ct);

    Task<(IReadOnlyList<TelefonReviewAdminListItem> Items, int TotalCount)> GetAdminPagedAsync(
        string? query,
        int page,
        int pageSize,
        CancellationToken ct);

    Task<IReadOnlyList<Telefon>> GetAdminPhoneSuggestionsAsync(int take, CancellationToken ct);
}
