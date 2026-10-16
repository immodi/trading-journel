using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.DTOs;
using Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Services;

/// <summary>
/// Claim names emitted by the API. Kept as short JWT names and mirrored in
/// <see cref="DependencyInjection.AddJwtAuthentication"/> so inbound mapping stays predictable.
/// </summary>
public static class JwtClaimNames
{
    public const string Subject = JwtRegisteredClaimNames.Sub;
    public const string Name = JwtRegisteredClaimNames.UniqueName;
    public const string Role = "role";
}

public class JwtTokenService(IConfiguration configuration) : IJwtTokenService
{
    public string GenerateToken(UserDto user)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secret = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings["Secret"] ?? string.Empty));

        var claims = new List<Claim>
        {
            new(JwtClaimNames.Subject, user.Id.ToString()),
            new(JwtClaimNames.Name, user.Username),
            new(JwtClaimNames.Role, jwtSettings["Role"] ?? "User"),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var credentials = new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(12),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
