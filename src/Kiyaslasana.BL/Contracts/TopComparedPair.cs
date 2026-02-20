namespace Kiyaslasana.BL.Contracts;

public sealed record TopComparedPair(
    string SlugLeft,
    string SlugRight,
    int VisitCount);
