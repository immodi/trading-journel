using Application.Interfaces.Services;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Services;

public class PasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<object> _passwordHasher = new();

    public string HashPassword(string password)
    {
        return _passwordHasher.HashPassword(null!, password);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        var result = _passwordHasher.VerifyHashedPassword(
            null!,
            passwordHash,
            password);

        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}