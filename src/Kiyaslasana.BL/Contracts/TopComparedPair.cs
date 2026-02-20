namespace Kiyaslasana.BL.Contracts;

public sealed record TopComparedPair(
    string CanonicalLeftSlug,
    string CanonicalRightSlug,
    int VisitCount);
