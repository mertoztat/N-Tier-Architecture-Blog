using BlogAPI.Entity.Dtos.Users;

namespace BlogAPI.Service.Services.Abstractions;

/// <summary>
/// User management for Admin / Superadmin. Failures are reported with exceptions from
/// BlogAPI.Core.Exceptions, not return values.
/// </summary>
public interface IUserService
{
    Task<List<UserDto>> GetAllUsersAsync();

    /// <summary>Superadmin may assign any role; Admin may only create plain "User" accounts.</summary>
    Task<UserDto> CreateUserAsync(UserAddDto dto);

    /// <summary>
    /// Permanently deletes the account. Admin may only delete plain "User" accounts, nobody can delete
    /// themselves, and a user who still has articles cannot be deleted.
    /// </summary>
    Task DeleteUserAsync(Guid id);
}
