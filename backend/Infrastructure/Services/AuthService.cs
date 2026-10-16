using Application.DTOs;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Mappers;
using Application.Requests.Auth;
using Application.Responses.Auth;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

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

    public async Task<RegisterResult> RegisterAsync(RegisterRequest request)
    {
        if (await userRepository.GetByEmailAsync(request.Email) is not null)
            return RegisterResult.EmailTaken();

        if (await userRepository.GetByUsernameAsync(request.Username) is not null)
            return RegisterResult.UsernameTaken();

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = passwordHasher.HashPassword(request.Password)
        };

        try
        {
            await userRepository.AddAsync(user);
            await userRepository.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Unique index violation from a concurrent registration.
            return RegisterResult.Conflict();
        }

        return RegisterResult.Success(user.ToDto());
    }
}
