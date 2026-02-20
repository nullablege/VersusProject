namespace Kiyaslasana.EL.Entities;

public sealed class CompareVisit
{
    public long Id { get; set; }
    public string CanonicalLeftSlug { get; set; } = string.Empty;
    public string CanonicalRightSlug { get; set; } = string.Empty;
    public DateTimeOffset VisitedAt { get; set; }
    public string? IpHash { get; set; }
}
