using Ganss.Xss;
using Kiyaslasana.BL.Abstractions;
using Kiyaslasana.BL.Contracts;
using Kiyaslasana.EL.Constants;
using Kiyaslasana.EL.Entities;

namespace Kiyaslasana.BL.Services;

public sealed class TelefonReviewService : ITelefonReviewService
{
    private readonly ITelefonReviewRepository _telefonReviewRepository;
    private readonly ITelefonRepository _telefonRepository;
    private readonly ITelefonService _telefonService;
    private readonly HtmlSanitizer _htmlSanitizer;

    public TelefonReviewService(
        ITelefonReviewRepository telefonReviewRepository,
        ITelefonRepository telefonRepository,
        ITelefonService telefonService)
    {
        _telefonReviewRepository = telefonReviewRepository;
        _telefonRepository = telefonRepository;
        _telefonService = telefonService;
        _htmlSanitizer = new HtmlSanitizer();
    }

    public async Task<TelefonReview?> GetByTelefonSlugAsync(string slug, CancellationToken ct)
    {
        var normalizedSlug = _telefonService.NormalizeSlug(slug);
        if (normalizedSlug.Length == 0)
        {
            return null;
        }

        return await _telefonReviewRepository.GetByTelefonSlugAsync(normalizedSlug, ct);
    }

    public async Task<TelefonReviewUpsertResult> UpsertAsync(string slug, TelefonReviewUpsertInput input, CancellationToken ct)
    {
        var normalizedSlug = _telefonService.NormalizeSlug(slug);
        if (normalizedSlug.Length == 0)
        {
            return new TelefonReviewUpsertResult(false, "Telefon slug zorunludur.", null);
        }

        var phone = await _telefonRepository.GetBySlugAsync(normalizedSlug, ct);
        if (phone is null)
        {
            return new TelefonReviewUpsertResult(false, "Inceleme eklenecek telefon bulunamadi.", null);
        }

        var normalizedRawContent = Normalize(input.RawContent);
        if (normalizedRawContent.Length == 0)
        {
            return new TelefonReviewUpsertResult(false, "Inceleme icerigi zorunludur.", null);
        }

        var existing = await _telefonReviewRepository.GetByTelefonSlugAsync(normalizedSlug, ct);
        var now = DateTimeOffset.UtcNow;

        if (existing is null)
        {
            existing = new TelefonReview
            {
                TelefonSlug = Truncate(normalizedSlug, TelefonReviewConstraints.TelefonSlugMaxLength),
                CreatedAt = now
            };

            ApplyInput(existing, input, normalizedRawContent, now);
            await _telefonReviewRepository.AddAsync(existing, ct);
            return new TelefonReviewUpsertResult(true, null, existing);
        }

        ApplyInput(existing, input, normalizedRawContent, now);
        await _telefonReviewRepository.UpdateAsync(existing, ct);
        return new TelefonReviewUpsertResult(true, null, existing);
    }

    public async Task<bool> DeleteAsync(string slug, CancellationToken ct)
    {
        var normalizedSlug = _telefonService.NormalizeSlug(slug);
        if (normalizedSlug.Length == 0)
        {
            return false;
        }

        var existing = await _telefonReviewRepository.GetByTelefonSlugAsync(normalizedSlug, ct);
        if (existing is null)
        {
            return false;
        }

        await _telefonReviewRepository.DeleteAsync(existing, ct);
        return true;
    }

    public Task<(IReadOnlyList<TelefonReviewAdminListItem> Items, int TotalCount)> GetAdminPagedAsync(
        string? query,
        int page,
        int pageSize,
        CancellationToken ct)
    {
        var safePage = Math.Max(page, 1);
        var safePageSize = Math.Clamp(pageSize, 1, 100);
        var skip = (safePage - 1) * safePageSize;

        return _telefonReviewRepository.GetAdminPagedAsync(query, skip, safePageSize, ct);
    }

    public async Task<IReadOnlyList<Telefon>> GetAdminPhoneSuggestionsAsync(int take, CancellationToken ct)
    {
        var safeTake = Math.Clamp(take, 1, 300);
        var latest = await _telefonRepository.GetLatestAsync(safeTake, ct);

        return latest
            .Where(x => !string.IsNullOrWhiteSpace(x.Slug))
            .Select(x => new Telefon
            {
                Slug = x.Slug,
                Marka = x.Marka,
                ModelAdi = x.ModelAdi
            })
            .ToArray();
    }

    private void ApplyInput(TelefonReview review, TelefonReviewUpsertInput input, string normalizedRawContent, DateTimeOffset now)
    {
        review.Title = TruncateOrNull(input.Title, TelefonReviewConstraints.TitleMaxLength);
        review.Excerpt = TruncateOrNull(input.Excerpt, TelefonReviewConstraints.ExcerptMaxLength);
        review.RawContent = normalizedRawContent;
        review.SanitizedContent = _htmlSanitizer.Sanitize(normalizedRawContent);
        review.SeoTitle = TruncateOrNull(input.SeoTitle, TelefonReviewConstraints.SeoTitleMaxLength);
        review.SeoDescription = TruncateOrNull(input.SeoDescription, TelefonReviewConstraints.SeoDescriptionMaxLength);
        review.UpdatedAt = now;
    }

    private static string Normalize(string? value)
    {
        return (value ?? string.Empty).Trim();
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

    private static string Truncate(string value, int maxLength)
    {
        if (value.Length <= maxLength)
        {
            return value;
        }

        return value[..maxLength];
    }
}
