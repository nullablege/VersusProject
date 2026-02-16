namespace Kiyaslasana.EL.Entities;

public sealed class TelefonReview
{
    public int Id { get; set; }

    public string TelefonSlug { get; set; } = string.Empty;

    public string? Title { get; set; }

    public string? Excerpt { get; set; }

    public string RawContent { get; set; } = string.Empty;

    public string SanitizedContent { get; set; } = string.Empty;

    public string? SeoTitle { get; set; }

    public string? SeoDescription { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
