using Application.DTOs;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Mappers;
using Application.Requests.Auth;
using Domain.Entities;

namespace Infrastructure.Services;

public class AuthService(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService) : IAuthService
{
    public async Task<string?> LoginAsync(LoginRequest request)
    {
        var user = await userRepository.GetByEmailAsync(request.Email);

        if (user is null)
            return null;

        var isValid = passwordHasher.VerifyPassword(
            request.Password,
            user.PasswordHash);

        return !isValid ? null : jwtTokenService.GenerateToken(user.ToDto());
    }

    public async Task<UserDto?> RegisterAsync(RegisterRequest request)
    {
        var existingUser = await userRepository.GetByEmailAsync(request.Email);

        if (existingUser is not null)
            return null;

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = passwordHasher.HashPassword(request.Password)
        };

        await userRepository.AddAsync(user);
        await userRepository.SaveChangesAsync();

        return user.ToDto();
    }
}