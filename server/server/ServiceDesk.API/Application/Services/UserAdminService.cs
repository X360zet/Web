using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using ServiceDesk.API.DTOs.Users;
using ServiceDesk.API.Infrastructure.Identity;

namespace ServiceDesk.API.Application.Services;

public class UserAdminService : IUserAdminService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<UserAdminService> _logger;

    public UserAdminService(UserManager<ApplicationUser> um, RoleManager<IdentityRole> rm, ILogger<UserAdminService> logger)
    {
        _userManager = um; _roleManager = rm; _logger = logger;
    }

    public async Task<IReadOnlyList<UserResponse>> GetAllAsync()
    {
        var users = new List<UserResponse>();
        foreach (var u in _userManager.Users)
        {
            var roles = await _userManager.GetRolesAsync(u);
            users.Add(new UserResponse
            {
                Id = u.Id, DisplayName = u.DisplayName, Email = u.Email!,
                Role = roles.FirstOrDefault() ?? "User"
            });
        }
        return users;
    }

    public async Task ChangeRoleAsync(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId) ?? throw new KeyNotFoundException("Пользователь не найден.");
        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);
        await _userManager.AddToRoleAsync(user, role);
        _logger.LogInformation("Роль пользователя {UserId} изменена на {Role}", userId, role);
    }
}