using Kiyaslasana.EL.Entities;

namespace Kiyaslasana.BL.Contracts;

public sealed record TelefonReviewUpsertResult(
    bool Success,
    string? ErrorMessage,
    TelefonReview? Review);
