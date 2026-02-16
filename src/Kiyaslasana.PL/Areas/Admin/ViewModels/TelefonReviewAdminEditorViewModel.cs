using Kiyaslasana.EL.Entities;
using Kiyaslasana.PL.Areas.Admin.Models;

namespace Kiyaslasana.PL.Areas.Admin.ViewModels;

public sealed class TelefonReviewAdminEditorViewModel
{
    public required string PageTitle { get; init; }

    public required string SubmitLabel { get; init; }

    public required bool IsEditMode { get; init; }

    public required TelefonReviewEditorInputModel Input { get; init; }

    public required IReadOnlyList<Telefon> PhoneSuggestions { get; init; }
}
