using BlogAPI.Entity.Dtos.Auth;
using FluentValidation;

namespace BlogAPI.Service.FluentValidations.Auth;

public class LoginDtoValidator : AbstractValidator<LoginDto>
{
    public LoginDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress().WithMessage("'E-posta' geçerli bir e-posta adresi değil.")
            .WithName("E-posta");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithName("Şifre");
    }
}
