using Kiyaslasana.EL.Entities;

namespace Kiyaslasana.BL.Abstractions;

public interface IBlogPostRepository
{
    Task<(IReadOnlyList<BlogPost> Items, int TotalCount)> GetAdminPagedAsync(int skip, int take, CancellationToken ct);

    Task<BlogPost?> GetByIdAsync(int id, CancellationToken ct);

    Task<BlogPost?> GetBySlugAsync(string slug, bool publishedOnly, CancellationToken ct);

    Task<(IReadOnlyList<BlogPost> Items, int TotalCount)> GetPublishedPagedAsync(int skip, int take, CancellationToken ct);

    Task<bool> SlugExistsAsync(string slug, int? excludeId, CancellationToken ct);

    Task AddAsync(BlogPost post, CancellationToken ct);

    Task UpdateAsync(BlogPost post, CancellationToken ct);

    Task<IReadOnlyList<BlogPost>> GetPublishedSitemapItemsAsync(CancellationToken ct);
}
