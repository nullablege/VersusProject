using FluentValidation;
using Kiyaslasana.PL.ViewModels.Auth;

namespace Kiyaslasana.PL.Validators.Auth;

public sealed class RegisterValidator : AbstractValidator<RegisterViewModel>
{
    public RegisterValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(6)
            .MaximumLength(200);

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password)
            .WithMessage("Sifre dogrulamasi sifre ile eslesmelidir.");
    }
}
