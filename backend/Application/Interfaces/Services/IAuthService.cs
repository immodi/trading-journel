using Application.DTOs;
using Application.Requests.Auth;
using Application.Responses.Auth;

namespace Application.Interfaces.Services;

public interface IAuthService
{
    Task<string?> LoginAsync(LoginRequest request);

    Task<RegisterResult> RegisterAsync(RegisterRequest request);
}
