using BlogAPI.Core.Constants;
using BlogAPI.Core.Exceptions;
using BlogAPI.Core.Services;
using BlogAPI.Data.UnitOfWorks;
using BlogAPI.Entity.Dtos.Users;
using BlogAPI.Entity.Entities;
using BlogAPI.Service.Services.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BlogAPI.Service.Services.Concretes;

public class UserService : IUserService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<AppRole> _roleManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public UserService(
        UserManager<AppUser> userManager,
        RoleManager<AppRole> roleManager,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        var users = await _userManager.Users
            .AsNoTracking()
            .OrderBy(u => u.FirstName).ThenBy(u => u.LastName)
            .ToListAsync();

        // One query per role (3) instead of one per user.
        var rolesByUser = new Dictionary<Guid, List<string>>();
        var roleNames = await _roleManager.Roles.Select(r => r.Name!).ToListAsync();
        foreach (var roleName in roleNames)
        {
            foreach (var member in await _userManager.GetUsersInRoleAsync(roleName))
            {
                if (!rolesByUser.TryGetValue(member.Id, out var list))
                {
                    rolesByUser[member.Id] = list = new List<string>();
                }
                list.Add(roleName);
            }
        }

        return users
            .Select(u => ToDto(u, rolesByUser.TryGetValue(u.Id, out var roles) ? roles : new List<string>()))
            .ToList();
    }

    public async Task<UserDto> CreateUserAsync(UserAddDto dto)
    {
        var roles = await ResolveRolesAsync(dto.Roles);
        EnsureCanAssign(roles);

        var email = dto.Email.Trim();
        if (await _userManager.FindByEmailAsync(email) is not null)
        {
            throw new ConflictException("Bu e-posta adresi zaten kayıtlı.");
        }

        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim()
        };

        var createResult = await _userManager.CreateAsync(user, dto.Password);
        if (!createResult.Succeeded)
        {
            throw new BadRequestException(Describe(createResult));
        }

        var roleResult = await _userManager.AddToRolesAsync(user, roles);
        if (!roleResult.Succeeded)
        {
            // Do not leave a user without roles behind.
            await _userManager.DeleteAsync(user);
            throw new BadRequestException(Describe(roleResult));
        }

        return ToDto(user, roles);
    }

    public async Task DeleteUserAsync(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString())
                   ?? throw new NotFoundException("Kullanıcı bulunamadı.");

        if (user.Id == _currentUser.UserId)
        {
            throw new BadRequestException("Kendi hesabınızı silemezsiniz.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        if (roles.Any(r => r != RoleNames.User) && !_currentUser.IsInRole(RoleNames.Superadmin))
        {
            throw new ForbiddenException("Yalnızca Superadmin, Admin veya Superadmin hesaplarını silebilir.");
        }

        // Article.UserId is Restrict (soft-deleted articles count too), so the delete would fail in the DB.
        var hasArticles = await _unitOfWork.GetRepository<Article>().AnyAsync(x => x.UserId == id);
        if (hasArticles)
        {
            throw new ConflictException("Bu kullanıcının makaleleri olduğu için silinemez.");
        }

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            throw new BadRequestException(Describe(result));
        }
    }

    /// <summary>Trims, de-duplicates and checks the role names; no roles means the default "User".</summary>
    private async Task<List<string>> ResolveRolesAsync(IEnumerable<string>? requested)
    {
        var names = (requested ?? Enumerable.Empty<string>())
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .Select(r => r.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (names.Count == 0)
        {
            return new List<string> { RoleNames.User };
        }

        var resolved = new List<string>();
        foreach (var name in names)
        {
            // FindByNameAsync is case-insensitive and gives us the canonical spelling.
            var role = await _roleManager.FindByNameAsync(name)
                       ?? throw new BadRequestException($"'{name}' adında bir rol bulunamadı.");
            resolved.Add(role.Name!);
        }

        return resolved;
    }

    private void EnsureCanAssign(IEnumerable<string> roles)
    {
        if (roles.Any(r => r != RoleNames.User) && !_currentUser.IsInRole(RoleNames.Superadmin))
        {
            throw new ForbiddenException("Yalnızca Superadmin, Admin veya Superadmin rolü atayabilir.");
        }
    }

    private static string Describe(IdentityResult result) =>
        string.Join(" ", result.Errors.Select(e => e.Description));

    private static UserDto ToDto(AppUser user, IList<string> roles) => new()
    {
        Id = user.Id,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Email = user.Email ?? string.Empty,
        Roles = roles.OrderBy(r => r).ToList()
    };
}
