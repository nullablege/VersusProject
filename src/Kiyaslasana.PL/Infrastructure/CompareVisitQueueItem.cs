namespace Kiyaslasana.PL.Infrastructure;

public sealed record CompareVisitQueueItem(
    string CanonicalLeftSlug,
    string CanonicalRightSlug,
    string? RemoteIpAddress);
