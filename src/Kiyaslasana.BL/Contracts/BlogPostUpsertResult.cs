using Kiyaslasana.EL.Entities;

namespace Kiyaslasana.BL.Contracts;

public sealed record BlogPostUpsertResult(
    bool Success,
    string? ErrorMessage,
    BlogPost? Post);
