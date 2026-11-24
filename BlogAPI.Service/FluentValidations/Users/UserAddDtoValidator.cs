using BlogAPI.Entity.Dtos.Users;
using FluentValidation;

namespace BlogAPI.Service.FluentValidations.Users;

// Only checks the shape of the request. Whether the roles exist, whether the caller may assign them
// and whether the e-mail is taken need Identity and stay in UserService.
public class UserAddDtoValidator : AbstractValidator<UserAddDto>
{
    public UserAddDtoValidator()
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

        // Same rules as RegisterDtoValidator / the Identity password options.
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(6)
            .Matches("[0-9]").WithMessage("'Şifre' en az bir rakam içermelidir.")
            .WithName("Şifre");

        RuleForEach(x => x.Roles)
            .NotEmpty()
            .MaximumLength(256)
            .WithName("Rol");
    }
}
