using Kiyaslasana.EL.Entities;

namespace Kiyaslasana.PL.ViewModels;

public sealed class TelefonListViewModel
{
    public required IReadOnlyList<Telefon> Phones { get; init; }
}
