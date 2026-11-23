namespace BlogAPI.Entity.Dtos.Auth;

// Validation rules: BlogAPI.Service/FluentValidations/Auth/LoginDtoValidator.cs
public class LoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
