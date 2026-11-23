namespace BlogAPI.Entity.Dtos.Auth;

// Validation rules: BlogAPI.Service/FluentValidations/Auth/RegisterDtoValidator.cs
public class RegisterDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
