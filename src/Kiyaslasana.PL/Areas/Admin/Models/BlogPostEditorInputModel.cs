namespace Kiyaslasana.PL.Areas.Admin.Models;

public sealed class BlogPostEditorInputModel
{
    public string Title { get; set; } = string.Empty;

    public string? Excerpt { get; set; }

    public string ContentRaw { get; set; } = string.Empty;

    public string? MetaTitle { get; set; }

    public string? MetaDescription { get; set; }

    public bool IsPublished { get; set; }

    public DateTimeOffset? PublishedAt { get; set; }
}
