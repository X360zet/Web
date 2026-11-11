using ServiceDesk.API.Infrastructure.Identity;

namespace ServiceDesk.API.Application.Services;

public interface ITokenService
{
    Task<string> GenerateTokenAsync(ApplicationUser user, string role);
}