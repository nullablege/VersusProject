using Kiyaslasana.BL.Abstractions;
using Kiyaslasana.BL.Contracts;
using Kiyaslasana.DAL.Data;
using Kiyaslasana.EL.Entities;
using Microsoft.EntityFrameworkCore;

namespace Kiyaslasana.DAL.Repositories;

public sealed class EfTelefonReviewRepository : ITelefonReviewRepository
{
    private readonly KiyaslasanaDbContext _dbContext;

    public EfTelefonReviewRepository(KiyaslasanaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<TelefonReview?> GetByTelefonSlugAsync(string telefonSlug, CancellationToken ct)
    {
        return _dbContext.TelefonReviews
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TelefonSlug == telefonSlug, ct);
    }

    public async Task<(IReadOnlyList<TelefonReviewAdminListItem> Items, int TotalCount)> GetAdminPagedAsync(
        string? searchTerm,
        int skip,
        int take,
        CancellationToken ct)
    {
        if (take <= 0)
        {
            return ([], 0);
        }

        var safeSkip = Math.Max(skip, 0);
        var safeSearchTerm = searchTerm?.Trim();

        var baseQuery =
            from review in _dbContext.TelefonReviews.AsNoTracking()
            join phone in _dbContext.Telefonlar.AsNoTracking() on review.TelefonSlug equals phone.Slug into phoneGroup
            from phone in phoneGroup.DefaultIfEmpty()
            select new TelefonReviewAdminListItem(
                review.Id,
                review.TelefonSlug,
                phone != null ? phone.Marka : null,
                phone != null ? phone.ModelAdi : null,
                review.Title,
                review.UpdatedAt);

        if (!string.IsNullOrWhiteSpace(safeSearchTerm))
        {
            baseQuery = baseQuery.Where(x =>
                x.TelefonSlug.Contains(safeSearchTerm)
                || (x.Marka != null && x.Marka.Contains(safeSearchTerm))
                || (x.ModelAdi != null && x.ModelAdi.Contains(safeSearchTerm)));
        }

        var totalCount = await baseQuery.CountAsync(ct);
        var maxSkip = totalCount <= 0 ? 0 : ((totalCount - 1) / take) * take;
        var effectiveSkip = Math.Min(safeSkip, maxSkip);

        var items = await baseQuery
            .OrderByDescending(x => x.UpdatedAt)
            .ThenBy(x => x.TelefonSlug)
            .Skip(effectiveSkip)
            .Take(take)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task AddAsync(TelefonReview review, CancellationToken ct)
    {
        await _dbContext.TelefonReviews.AddAsync(review, ct);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(TelefonReview review, CancellationToken ct)
    {
        _dbContext.TelefonReviews.Update(review);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(TelefonReview review, CancellationToken ct)
    {
        _dbContext.TelefonReviews.Remove(review);
        await _dbContext.SaveChangesAsync(ct);
    }
}
