namespace Kiyaslasana.BL.Contracts;

public sealed record TelefonReviewAdminListItem(
    int Id,
    string TelefonSlug,
    string? Marka,
    string? ModelAdi,
    string? ReviewTitle,
    DateTimeOffset UpdatedAt);
