namespace Kiyaslasana.BL.Contracts;

public sealed record CompareParseResult(
    bool IsValid,
    string? ErrorMessage,
    IReadOnlyList<string> RequestedSlugs,
    IReadOnlyList<string> CanonicalSlugs,
    int MaxAllowed);
