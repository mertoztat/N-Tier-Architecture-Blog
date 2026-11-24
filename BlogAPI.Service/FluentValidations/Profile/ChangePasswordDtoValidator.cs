using BlogAPI.Entity.Dtos.Profile;
using FluentValidation;

namespace BlogAPI.Service.FluentValidations.Profile;

// Whether the current password is correct is checked by Identity in ProfileService.
public class ChangePasswordDtoValidator : AbstractValidator<ChangePasswordDto>
{
    public ChangePasswordDtoValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty()
            .WithName("Mevcut şifre");

        // Same rules as RegisterDtoValidator / the Identity password options.
        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .MinimumLength(6)
            .Matches("[0-9]").WithMessage("'Yeni şifre' en az bir rakam içermelidir.")
            .NotEqual(x => x.CurrentPassword).WithMessage("'Yeni şifre' mevcut şifreyle aynı olamaz.")
            .WithName("Yeni şifre");
    }
}
