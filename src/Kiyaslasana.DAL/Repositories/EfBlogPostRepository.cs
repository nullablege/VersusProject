using Kiyaslasana.BL.Abstractions;
using Kiyaslasana.DAL.Data;
using Kiyaslasana.EL.Entities;
using Microsoft.EntityFrameworkCore;

namespace Kiyaslasana.DAL.Repositories;

public sealed class EfBlogPostRepository : IBlogPostRepository
{
    private readonly KiyaslasanaDbContext _dbContext;

    public EfBlogPostRepository(KiyaslasanaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<(IReadOnlyList<BlogPost> Items, int TotalCount)> GetAdminPagedAsync(int skip, int take, CancellationToken ct)
    {
        if (take <= 0)
        {
            return ([], 0);
        }

        var safeSkip = Math.Max(skip, 0);
        var baseQuery = _dbContext.BlogPosts.AsNoTracking();
        var totalCount = await baseQuery.CountAsync(ct);
        var maxSkip = totalCount <= 0 ? 0 : ((totalCount - 1) / take) * take;
        var effectiveSkip = Math.Min(safeSkip, maxSkip);

        var items = await baseQuery
            .OrderByDescending(x => x.UpdatedAt)
            .ThenByDescending(x => x.Id)
            .Skip(effectiveSkip)
            .Take(take)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public Task<BlogPost?> GetByIdAsync(int id, CancellationToken ct)
    {
        return _dbContext.BlogPosts.FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<BlogPost?> GetBySlugAsync(string slug, bool publishedOnly, CancellationToken ct)
    {
        var query = _dbContext.BlogPosts.AsNoTracking().Where(x => x.Slug == slug);
        if (publishedOnly)
        {
            var now = DateTimeOffset.UtcNow;
            query = query.Where(x => x.IsPublished && x.PublishedAt != null && x.PublishedAt <= now);
        }

        return await query.FirstOrDefaultAsync(ct);
    }

    public async Task<(IReadOnlyList<BlogPost> Items, int TotalCount)> GetPublishedPagedAsync(int skip, int take, CancellationToken ct)
    {
        if (take <= 0)
        {
            return ([], 0);
        }

        var safeSkip = Math.Max(skip, 0);
        var now = DateTimeOffset.UtcNow;
        var baseQuery = _dbContext.BlogPosts
            .AsNoTracking()
            .Where(x => x.IsPublished && x.PublishedAt != null && x.PublishedAt <= now);

        var totalCount = await baseQuery.CountAsync(ct);
        var maxSkip = totalCount <= 0 ? 0 : ((totalCount - 1) / take) * take;
        var effectiveSkip = Math.Min(safeSkip, maxSkip);
        var items = await baseQuery
            .OrderByDescending(x => x.PublishedAt)
            .ThenByDescending(x => x.Id)
            .Skip(effectiveSkip)
            .Take(take)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public Task<bool> SlugExistsAsync(string slug, int? excludeId, CancellationToken ct)
    {
        return _dbContext.BlogPosts.AsNoTracking().AnyAsync(
            x => x.Slug == slug && (!excludeId.HasValue || x.Id != excludeId.Value),
            ct);
    }

    public async Task AddAsync(BlogPost post, CancellationToken ct)
    {
        await _dbContext.BlogPosts.AddAsync(post, ct);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(BlogPost post, CancellationToken ct)
    {
        _dbContext.BlogPosts.Update(post);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<BlogPost>> GetPublishedSitemapItemsAsync(CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        return await _dbContext.BlogPosts
            .AsNoTracking()
            .Where(x => x.IsPublished && x.PublishedAt != null && x.PublishedAt <= now)
            .OrderByDescending(x => x.UpdatedAt)
            .Select(x => new BlogPost
            {
                Slug = x.Slug,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync(ct);
    }
}
