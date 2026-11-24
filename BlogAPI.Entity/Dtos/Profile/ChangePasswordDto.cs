namespace BlogAPI.Entity.Dtos.Profile;

// Validation rules: BlogAPI.Service/FluentValidations/Profile/ChangePasswordDtoValidator.cs
public class ChangePasswordDto
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
