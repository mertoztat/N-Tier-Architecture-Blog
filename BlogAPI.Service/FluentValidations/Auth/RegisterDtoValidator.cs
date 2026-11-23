using BlogAPI.Entity.Dtos.Auth;
using FluentValidation;

namespace BlogAPI.Service.FluentValidations.Auth;

public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    public RegisterDtoValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(100)
            .WithName("Ad");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(100)
            .WithName("Soyad");

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress().WithMessage("'E-posta' geçerli bir e-posta adresi değil.")
            .MaximumLength(256)
            .WithName("E-posta");

        // Mirrors the Identity password options in DataLayerExtensions (length 6, at least one digit)
        // so the user gets a Turkish message before Identity is even called.
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(6)
            .Matches("[0-9]").WithMessage("'Şifre' en az bir rakam içermelidir.")
            .WithName("Şifre");
    }
}
