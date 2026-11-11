using ServiceDesk.API.Application.DTOs;

namespace ServiceDesk.API.Application.Services;

public interface IAuthService
{
    Task<RegisterResultDto> RegisterAsync(RegisterDto request);
    Task<LoginResultDto> LoginAsync(LoginDto request);
}