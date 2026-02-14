using Kiyaslasana.EL.Entities;

namespace Kiyaslasana.PL.ViewModels;

public sealed class KarsilastirBuilderViewModel
{
    public required int MaxAllowed { get; init; }
    public required IReadOnlyList<string> SlugInputs { get; init; }
    public required IReadOnlyList<Telefon> SuggestedPhones { get; init; }
}
