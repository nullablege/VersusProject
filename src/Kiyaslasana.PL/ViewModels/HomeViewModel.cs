using Kiyaslasana.EL.Entities;

namespace Kiyaslasana.PL.ViewModels;

public sealed class HomeViewModel
{
    public required IReadOnlyList<Telefon> LatestPhones { get; init; }
}
