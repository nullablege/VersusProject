namespace Kiyaslasana.BL.Contracts;

public sealed record RelatedComparisonLink(
    string CurrentSlug,
    string OtherSlug,
    string CanonicalLeftSlug,
    string CanonicalRightSlug,
    string UrlPath,
    string OtherTitle,
    string? OtherImageUrl);
