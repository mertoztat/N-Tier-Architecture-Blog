using Microsoft.AspNetCore.Identity;

namespace BlogAPI.Entity.Entities;

public class AppUser : IdentityUser<Guid>
{
    public string FirstName { get; set; }
    public string LastName { get; set; }

    // Optional: a freshly registered user has no profile picture yet.
    public Guid? ImageId { get; set; }
    public Image? Image { get; set; }
    public ICollection<Article> Articles { get; set; }
}
