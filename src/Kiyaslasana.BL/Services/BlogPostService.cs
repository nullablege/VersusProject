using System.Text.RegularExpressions;
using Ganss.Xss;
using Kiyaslasana.BL.Abstractions;
using Kiyaslasana.BL.Contracts;
using Kiyaslasana.BL.Helpers;
using Kiyaslasana.BL.SeoFilters;
using Kiyaslasana.EL.Constants;
using Kiyaslasana.EL.Entities;

namespace Kiyaslasana.BL.Services;

public sealed class BlogPostService : IBlogPostService
{
    private const string DefaultAuthorName = "Kiyaslasana Editorial";

    private readonly IBlogPostRepository _blogPostRepository;
    private readonly ITelefonService _telefonService;
    private readonly HtmlSanitizer _htmlSanitizer;

    public BlogPostService(IBlogPostRepository blogPostRepository, ITelefonService telefonService)
    {
        _blogPostRepository = blogPostRepository;
        _telefonService = telefonService;
        _htmlSanitizer = new HtmlSanitizer();
    }

    public Task<(IReadOnlyList<BlogPost> Items, int TotalCount)> GetAdminPagedAsync(int page, int pageSize, CancellationToken ct)
    {
        var safePageSize = Math.Clamp(pageSize, 1, 100);
        var safePage = Math.Max(page, 1);
        var skip = (safePage - 1) * safePageSize;
        return _blogPostRepository.GetAdminPagedAsync(skip, safePageSize, ct);
    }

    public Task<BlogPost?> GetAdminByIdAsync(int id, CancellationToken ct)
    {
        return _blogPostRepository.GetByIdAsync(id, ct);
    }

    public async Task<BlogPostUpsertResult> CreateAsync(BlogPostUpsertInput input, CancellationToken ct)
    {
        var normalizedTitle = Normalize(input.Title);
        if (normalizedTitle.Length == 0)
        {
            return new BlogPostUpsertResult(false, "Baslik zorunludur.", null);
        }

        var now = DateTimeOffset.UtcNow;
        var slugBase = SlugHelper.ToSlug(normalizedTitle);
        if (slugBase.Length == 0)
        {
            slugBase = "blog-yazisi";
        }

        var slug = await BuildUniqueSlugAsync(slugBase, excludeId: null, ct);
        var isPublished = input.IsPublished;
        DateTimeOffset? publishedAt = isPublished ? input.PublishedAt ?? now : null;

        var post = new BlogPost
        {
            Title = Truncate(normalizedTitle, BlogPostConstraints.TitleMaxLength),
            Slug = Truncate(slug, BlogPostConstraints.SlugMaxLength),
            Excerpt = TruncateOrNull(input.Excerpt, BlogPostConstraints.ExcerptMaxLength),
            ContentRaw = input.ContentRaw ?? string.Empty,
            ContentSanitized = Sanitize(input.ContentRaw),
            MetaTitle = TruncateOrNull(input.MetaTitle, BlogPostConstraints.MetaTitleMaxLength),
            MetaDescription = TruncateOrNull(input.MetaDescription, BlogPostConstraints.MetaDescriptionMaxLength),
            IsPublished = isPublished,
            PublishedAt = publishedAt,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _blogPostRepository.AddAsync(post, ct);
        return new BlogPostUpsertResult(true, null, post);
    }

    public async Task<BlogPostUpsertResult> UpdateAsync(int id, BlogPostUpsertInput input, CancellationToken ct)
    {
        var existing = await _blogPostRepository.GetByIdAsync(id, ct);
        if (existing is null)
        {
            return new BlogPostUpsertResult(false, "Blog yazisi bulunamadi.", null);
        }

        var normalizedTitle = Normalize(input.Title);
        if (normalizedTitle.Length == 0)
        {
            return new BlogPostUpsertResult(false, "Baslik zorunludur.", null);
        }

        var slugBase = SlugHelper.ToSlug(normalizedTitle);
        if (slugBase.Length == 0)
        {
            slugBase = "blog-yazisi";
        }

        var slug = await BuildUniqueSlugAsync(slugBase, excludeId: id, ct);
        var now = DateTimeOffset.UtcNow;
        var isPublished = input.IsPublished;
        DateTimeOffset? publishedAt = isPublished ? input.PublishedAt ?? existing.PublishedAt ?? now : null;

        existing.Title = Truncate(normalizedTitle, BlogPostConstraints.TitleMaxLength);
        existing.Slug = Truncate(slug, BlogPostConstraints.SlugMaxLength);
        existing.Excerpt = TruncateOrNull(input.Excerpt, BlogPostConstraints.ExcerptMaxLength);
        existing.ContentRaw = input.ContentRaw ?? string.Empty;
        existing.ContentSanitized = Sanitize(input.ContentRaw);
        existing.MetaTitle = TruncateOrNull(input.MetaTitle, BlogPostConstraints.MetaTitleMaxLength);
        existing.MetaDescription = TruncateOrNull(input.MetaDescription, BlogPostConstraints.MetaDescriptionMaxLength);
        existing.IsPublished = isPublished;
        existing.PublishedAt = publishedAt;
        existing.UpdatedAt = now;

        await _blogPostRepository.UpdateAsync(existing, ct);
        return new BlogPostUpsertResult(true, null, existing);
    }

    public Task<(IReadOnlyList<BlogPost> Items, int TotalCount)> GetPublishedPagedAsync(int page, int pageSize, CancellationToken ct)
    {
        var safePageSize = Math.Clamp(pageSize, 1, 100);
        var safePage = Math.Max(page, 1);
        var skip = (safePage - 1) * safePageSize;
        return _blogPostRepository.GetPublishedPagedAsync(skip, safePageSize, ct);
    }

    public Task<BlogPost?> GetPublishedBySlugAsync(string slug, CancellationToken ct)
    {
        var normalizedSlug = SlugHelper.ToSlug(slug);
        if (normalizedSlug.Length == 0)
        {
            return Task.FromResult<BlogPost?>(null);
        }

        return _blogPostRepository.GetBySlugAsync(normalizedSlug, publishedOnly: true, ct);
    }

    public Task<IReadOnlyList<BlogPost>> GetPublishedSitemapItemsAsync(CancellationToken ct)
    {
        return _blogPostRepository.GetPublishedSitemapItemsAsync(ct);
    }

    public async Task<IReadOnlyList<BlogInternalLink>> BuildInternalLinksAsync(BlogPost post, CancellationToken ct)
    {
        var contentText = $"{post.Title} {post.Excerpt} {StripHtml(post.ContentSanitized)}".ToLowerInvariant();
        var links = new List<BlogInternalLink>();
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var filter in SeoFilterRegistry.GetAll())
        {
            var slugToken = filter.Slug.ToLowerInvariant();
            var titleToken = filter.Title.ToLowerInvariant();
            if (!contentText.Contains(slugToken, StringComparison.Ordinal)
                && !contentText.Contains(titleToken, StringComparison.Ordinal))
            {
                continue;
            }

            var url = $"/telefonlar/{filter.Slug}";
            if (seen.Add(url))
            {
                links.Add(new BlogInternalLink(url, filter.Title, "filter"));
            }
        }

        var phones = await _telefonService.GetLatestAsync(50, ct);
        var matchedPhones = phones
            .Where(x => !string.IsNullOrWhiteSpace(x.Slug))
            .Where(x => ContainsPhoneName(contentText, x))
            .Take(4)
            .ToArray();

        for (var i = 0; i < matchedPhones.Length; i++)
        {
            for (var j = i + 1; j < matchedPhones.Length; j++)
            {
                var left = matchedPhones[i].Slug!;
                var right = matchedPhones[j].Slug!;
                var canonicalLeft = string.Compare(left, right, StringComparison.Ordinal) <= 0 ? left : right;
                var canonicalRight = canonicalLeft == left ? right : left;
                var url = $"/karsilastir/{canonicalLeft}-vs-{canonicalRight}";

                if (!seen.Add(url))
                {
                    continue;
                }

                links.Add(new BlogInternalLink(
                    url,
                    $"{BuildPhoneLabel(matchedPhones[i])} vs {BuildPhoneLabel(matchedPhones[j])}",
                    "compare"));

                if (links.Count >= 12)
                {
                    return links;
                }
            }
        }

        return links;
    }

    public static string GetDefaultAuthorName()
    {
        return DefaultAuthorName;
    }

    private async Task<string> BuildUniqueSlugAsync(string slugBase, int? excludeId, CancellationToken ct)
    {
        var candidate = Truncate(slugBase, BlogPostConstraints.SlugMaxLength);
        if (!await _blogPostRepository.SlugExistsAsync(candidate, excludeId, ct))
        {
            return candidate;
        }

        for (var i = 2; i <= 9999; i++)
        {
            var suffix = "-" + i;
            var maxBaseLength = BlogPostConstraints.SlugMaxLength - suffix.Length;
            var safeBase = Truncate(slugBase, Math.Max(maxBaseLength, 1));
            candidate = safeBase + suffix;

            if (!await _blogPostRepository.SlugExistsAsync(candidate, excludeId, ct))
            {
                return candidate;
            }
        }

        return Truncate($"{slugBase}-{Guid.NewGuid():N}", BlogPostConstraints.SlugMaxLength);
    }

    private string Sanitize(string? html)
    {
        return _htmlSanitizer.Sanitize(html ?? string.Empty);
    }

    private static string Normalize(string? value)
    {
        return (value ?? string.Empty).Trim();
    }

    private static string Truncate(string value, int maxLength)
    {
        if (value.Length <= maxLength)
        {
            return value;
        }

        return value[..maxLength];
    }

    private static string? TruncateOrNull(string? value, int maxLength)
    {
        var normalized = Normalize(value);
        if (normalized.Length == 0)
        {
            return null;
        }

        return Truncate(normalized, maxLength);
    }

    private static string StripHtml(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return string.Empty;
        }

        return Regex.Replace(html, "<.*?>", " ");
    }

    private static bool ContainsPhoneName(string source, Telefon phone)
    {
        var model = Normalize(phone.ModelAdi).ToLowerInvariant();
        if (model.Length > 0 && source.Contains(model, StringComparison.Ordinal))
        {
            return true;
        }

        var full = Normalize($"{phone.Marka} {phone.ModelAdi}").ToLowerInvariant();
        return full.Length > 0 && source.Contains(full, StringComparison.Ordinal);
    }

    private static string BuildPhoneLabel(Telefon phone)
    {
        return string.Join(' ', new[] { phone.Marka, phone.ModelAdi }.Where(x => !string.IsNullOrWhiteSpace(x))).Trim();
    }
}
