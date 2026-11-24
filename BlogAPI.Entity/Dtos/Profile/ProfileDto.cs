namespace BlogAPI.Entity.Dtos.Profile;

public class ProfileDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public IList<string> Roles { get; set; } = new List<string>();

    // Relative path under /images, e.g. "users-images/abc_123.jpg"; null when there is no photo.
    public string? ImageFileName { get; set; }
}
