using FluentValidation;
using Kiyaslasana.PL.ViewModels.Auth;

namespace Kiyaslasana.PL.Validators.Auth;

public sealed class LoginValidator : AbstractValidator<LoginViewModel>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MaximumLength(200);
    }
}
