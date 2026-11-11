using ServiceDesk.API.DTOs.Users;
namespace ServiceDesk.API.Application.Services;
public interface IUserAdminService
{
    Task<IReadOnlyList<UserResponse>> GetAllAsync();
    Task ChangeRoleAsync(string userId, string role);
}