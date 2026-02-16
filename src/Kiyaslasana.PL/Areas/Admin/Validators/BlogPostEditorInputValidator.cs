using FluentValidation;
using Kiyaslasana.EL.Constants;
using Kiyaslasana.PL.Areas.Admin.Models;

namespace Kiyaslasana.PL.Areas.Admin.Validators;

public sealed class BlogPostEditorInputValidator : AbstractValidator<BlogPostEditorInputModel>
{
    public BlogPostEditorInputValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(BlogPostConstraints.TitleMaxLength);

        RuleFor(x => x.Excerpt)
            .MaximumLength(BlogPostConstraints.ExcerptMaxLength);

        RuleFor(x => x.ContentRaw)
            .NotEmpty();

        RuleFor(x => x.MetaTitle)
            .MaximumLength(BlogPostConstraints.MetaTitleMaxLength);

        RuleFor(x => x.MetaDescription)
            .MaximumLength(BlogPostConstraints.MetaDescriptionMaxLength);
    }
}
