namespace Kiyaslasana.EL.Entities;

public sealed class CompareVisit
{
    public long Id { get; set; }
    public string SlugLeft { get; set; } = string.Empty;
    public string SlugRight { get; set; } = string.Empty;
    public DateTimeOffset VisitedAt { get; set; }
    public string? IPHash { get; set; }
}
