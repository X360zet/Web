using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ServiceDesk.API.Application.Services;
using ServiceDesk.API.Infrastructure.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ServiceDesk.API.Infrastructure.Auth;

public class TokenService : ITokenService
{
    private readonly IConfiguration _config;

    public TokenService(IConfiguration config) => _config = config;

public Task<string> GenerateTokenAsync(ApplicationUser user, string role)
{
    var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, user.Id),
        new(ClaimTypes.Email, user.Email!),
        new("displayName", user.DisplayName),
        new(ClaimTypes.Role, role),
        new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

    var jwtKey = _config["Jwt:Key"]!;
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: _config["Jwt:Issuer"],
        audience: _config["Jwt:Audience"],
        claims: claims,
        expires: DateTime.UtcNow.AddHours(2),
        signingCredentials: creds
    );

    return Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
    }
}