using Kiyaslasana.PL.Areas.Admin.Models;

namespace Kiyaslasana.PL.Areas.Admin.ViewModels;

public sealed class BlogAdminEditorViewModel
{
    public required string PageTitle { get; init; }

    public required string SubmitLabel { get; init; }

    public required int? Id { get; init; }

    public required BlogPostEditorInputModel Input { get; init; }
}
