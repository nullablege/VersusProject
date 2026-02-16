namespace Kiyaslasana.BL.Contracts;

public sealed record BlogPostUpsertInput(
    string Title,
    string? Excerpt,
    string ContentRaw,
    string? MetaTitle,
    string? MetaDescription,
    bool IsPublished,
    DateTimeOffset? PublishedAt);
