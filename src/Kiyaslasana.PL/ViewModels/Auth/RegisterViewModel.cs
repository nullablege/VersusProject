namespace Kiyaslasana.PL.ViewModels.Auth;

public sealed class RegisterViewModel
{
    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string ConfirmPassword { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}
