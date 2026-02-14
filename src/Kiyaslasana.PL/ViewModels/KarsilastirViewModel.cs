using Kiyaslasana.EL.Entities;

namespace Kiyaslasana.PL.ViewModels;

public sealed class KarsilastirViewModel
{
    public required IReadOnlyList<Telefon> Phones { get; init; }
    public required string CanonicalComparePath { get; init; }
    public required int MaxAllowed { get; init; }
}
