namespace BlogAPI.Core.Services;

/// <summary>
/// The user making the current request. Lives in Core so that both the Data layer
/// (audit fields) and the Service layer (ownership checks) can use it without depending on HTTP.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>Null when the request is anonymous (or outside an HTTP request, e.g. migrations).</summary>
    Guid? UserId { get; }

    string? UserName { get; }

    bool IsInRole(string role);
}
