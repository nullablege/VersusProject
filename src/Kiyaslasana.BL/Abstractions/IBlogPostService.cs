using Kiyaslasana.BL.Contracts;
using Kiyaslasana.EL.Entities;

namespace Kiyaslasana.BL.Abstractions;

public interface IBlogPostService
{
    Task<(IReadOnlyList<BlogPost> Items, int TotalCount)> GetAdminPagedAsync(int page, int pageSize, CancellationToken ct);

    Task<BlogPost?> GetAdminByIdAsync(int id, CancellationToken ct);

    Task<BlogPostUpsertResult> CreateAsync(BlogPostUpsertInput input, CancellationToken ct);

    Task<BlogPostUpsertResult> UpdateAsync(int id, BlogPostUpsertInput input, CancellationToken ct);

    Task<(IReadOnlyList<BlogPost> Items, int TotalCount)> GetPublishedPagedAsync(int page, int pageSize, CancellationToken ct);

    Task<BlogPost?> GetPublishedBySlugAsync(string slug, CancellationToken ct);

    Task<IReadOnlyList<BlogPost>> GetPublishedSitemapItemsAsync(CancellationToken ct);

    Task<IReadOnlyList<BlogInternalLink>> BuildInternalLinksAsync(BlogPost post, CancellationToken ct);

    Task<string> BuildTelefonSlugLinksAsync(string sanitizedHtml, CancellationToken ct);

    Task<IReadOnlyList<BlogPost>> GetLatestPublishedMentioningTelefonSlugAsync(string telefonSlug, int take, CancellationToken ct);
}
