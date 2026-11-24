using BlogAPI.Entity.Dtos.Profile;
using BlogAPI.Service.FluentValidations.Common;
using FluentValidation;

namespace BlogAPI.Service.FluentValidations.Profile;

public class ProfilePhotoDtoValidator : AbstractValidator<ProfilePhotoDto>
{
    public ProfilePhotoDtoValidator()
    {
        RuleFor(x => x.Photo)
            .NotNull().WithMessage("Bir fotoğraf seçmelisiniz.");

        RuleFor(x => x.Photo!)
            .SetValidator(new ImageFileValidator())
            .When(x => x.Photo is not null);
    }
}
