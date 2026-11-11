using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using ServiceDesk.API.Application.DTOs;
using ServiceDesk.API.Infrastructure.Identity;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceDesk.API.Application.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ITokenService tokenService,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<RegisterResultDto> RegisterAsync(RegisterDto request)
    {
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            DisplayName = request.DisplayName
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogWarning("Registration failed for {Email}: {Errors}", request.Email, errors);
            throw new InvalidOperationException(errors);
        }

        await _userManager.AddToRoleAsync(user, "User");
        
        // ✅ Вызываем с явной ролью
        var token = await _tokenService.GenerateTokenAsync(user, "User");
        return new RegisterResultDto { AccessToken = token };
    }

public async Task<LoginResultDto> LoginAsync(LoginDto request)
{
    // 🔹 Ищем и по UserName, и по Email (для поддержки простых логинов)
    var user = await _userManager.FindByNameAsync(request.Email) 
            ?? await _userManager.FindByEmailAsync(request.Email);
    
    if (user == null)
    {
        _logger.LogWarning("Login attempt with non-existent user: {Login}", request.Email);
        throw new UnauthorizedAccessException("Invalid credentials.");
    }

    var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
    if (!result.Succeeded)
    {
        _logger.LogWarning("Failed password attempt for {Login}", request.Email);
        throw new UnauthorizedAccessException("Invalid credentials.");
    }

    var roles = await _userManager.GetRolesAsync(user);
    var role = roles.FirstOrDefault() ?? "User";

    var token = await _tokenService.GenerateTokenAsync(user, role);

    return new LoginResultDto
    {
        AccessToken = token,
        User = new UserInfoDto
        {
            Id = user.Id,
            DisplayName = user.DisplayName,
            Email = user.Email!,
            Role = role
        }
    };
  }
}