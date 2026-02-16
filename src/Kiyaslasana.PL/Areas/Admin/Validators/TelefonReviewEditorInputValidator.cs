using FluentValidation;
using Kiyaslasana.EL.Constants;
using Kiyaslasana.PL.Areas.Admin.Models;

namespace Kiyaslasana.PL.Areas.Admin.Validators;

public sealed class TelefonReviewEditorInputValidator : AbstractValidator<TelefonReviewEditorInputModel>
{
    public TelefonReviewEditorInputValidator()
    {
        RuleFor(x => x.TelefonSlug)
            .NotEmpty()
            .MaximumLength(TelefonReviewConstraints.TelefonSlugMaxLength)
            .Matches("^[a-z0-9-]+$");

        RuleFor(x => x.Title)
            .MaximumLength(TelefonReviewConstraints.TitleMaxLength);

        RuleFor(x => x.Excerpt)
            .MaximumLength(TelefonReviewConstraints.ExcerptMaxLength);

        RuleFor(x => x.RawContent)
            .NotEmpty();

        RuleFor(x => x.SeoTitle)
            .MaximumLength(TelefonReviewConstraints.SeoTitleMaxLength);

        RuleFor(x => x.SeoDescription)
            .MaximumLength(TelefonReviewConstraints.SeoDescriptionMaxLength);
    }
}
