using Application.DTOs;
using Application.Interfaces.Services;
using Application.Requests.User;
using Domain.Entities;

namespace Application.Mappers;

public static class UserMapper
{
    public static UserDto ToDto(this User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            CreatedAt = user.CreatedAt,
            Trades = user.Trades.Select(t => t.ToDto()).ToList(),
        };
    }

    public static User ToEntity(this CreateUserRequest request, IPasswordHasher passwordHasher)
    {
        return new User
        {
            Username = request.Username,
            Email = request.Email,
            CreatedAt = DateTime.UtcNow,
            PasswordHash = passwordHasher.HashPassword(request.Password),
        };
    }
    
    public static void UpdateUser(
        this UpdateUserRequest request,
        User user,
        IPasswordHasher passwordHasher)
    {
        if (request.Username is not null)
            user.Username = request.Username;

        if (request.Email is not null)
            user.Email = request.Email;

        if (request.Password is not null)
            user.PasswordHash = passwordHasher.HashPassword(request.Password);
    }
}