namespace Kiyaslasana.PL.Areas.Admin.Models;

public sealed class TelefonReviewEditorInputModel
{
    public string TelefonSlug { get; set; } = string.Empty;

    public string? Title { get; set; }

    public string? Excerpt { get; set; }

    public string RawContent { get; set; } = string.Empty;

    public string? SeoTitle { get; set; }

    public string? SeoDescription { get; set; }
}
