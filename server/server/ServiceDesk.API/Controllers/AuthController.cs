using Microsoft.AspNetCore.Mvc;
using ServiceDesk.API.Application.DTOs;
using ServiceDesk.API.Application.Services;

namespace ServiceDesk.API.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterResultDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> Register([FromBody] RegisterDto request)
    {
        var result = await _authService.RegisterAsync(request);
        return Ok(result);
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResultDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> Login([FromBody] LoginDto request)
    {
        var result = await _authService.LoginAsync(request);
        return Ok(result);
    }

    [HttpGet("me")]
    [ProducesResponseType(typeof(UserInfoDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public IActionResult GetMe()
    {
        var id = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        var displayName = User.FindFirst("displayName")?.Value;
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

        if (string.IsNullOrEmpty(id))
            return Unauthorized();

        var user = new UserInfoDto
        {
            Id = id,
            DisplayName = displayName ?? email!,
            Email = email ?? "unknown@email.com", // ✅ Исправлено: используем локальную переменную email
            Role = role ?? "User"
        };

        return Ok(user);
    }
}