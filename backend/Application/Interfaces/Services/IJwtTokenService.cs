using Application.DTOs;

namespace Application.Interfaces.Services;

public interface IJwtTokenService
{
    string GenerateToken(UserDto user);
}