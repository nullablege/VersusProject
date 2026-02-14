using Kiyaslasana.EL.Entities;

namespace Kiyaslasana.BL.Contracts;

public sealed record CompareResolveResult(
    bool IsValid,
    string? ErrorMessage,
    IReadOnlyList<string> CanonicalSlugs,
    IReadOnlyList<Telefon> Phones,
    int MaxAllowed);
