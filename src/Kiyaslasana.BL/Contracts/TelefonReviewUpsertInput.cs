namespace Kiyaslasana.BL.Contracts;

public sealed record TelefonReviewUpsertInput(
    string? Title,
    string? Excerpt,
    string RawContent,
    string? SeoTitle,
    string? SeoDescription);
