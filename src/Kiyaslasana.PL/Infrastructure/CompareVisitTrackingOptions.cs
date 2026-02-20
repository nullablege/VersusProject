namespace Kiyaslasana.PL.Infrastructure;

public sealed class CompareVisitTrackingOptions
{
    public const string SectionName = "CompareVisitTracking";

    public string IpHashSalt { get; set; } = string.Empty;
}
