namespace BlogAPI.Entity.Dtos.Auth;

public class RegisterResultDto
{
    public bool Succeeded { get; set; }

    /// <summary>True when the e-mail address is already registered (maps to 409 Conflict).</summary>
    public bool EmailTaken { get; set; }

    public Guid? UserId { get; set; }

    /// <summary>Identity validation errors, e.g. password too short.</summary>
    public IReadOnlyList<string> Errors { get; set; } = Array.Empty<string>();
}
