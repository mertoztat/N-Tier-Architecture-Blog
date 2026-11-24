namespace BlogAPI.Entity.Dtos.Users;

// Validation rules: BlogAPI.Service/FluentValidations/Users/UserAddDtoValidator.cs
public class UserAddDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    // Role names ("Superadmin", "Admin", "User"). Empty means just "User".
    public IList<string> Roles { get; set; } = new List<string>();
}
