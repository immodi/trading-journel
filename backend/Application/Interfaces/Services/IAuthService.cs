using Application.DTOs;
using Application.Requests.Auth;

namespace Application.Interfaces.Services;

public interface IAuthService
{
    
    Task<string?> LoginAsync(LoginRequest request); 
    Task<UserDto?> RegisterAsync(RegisterRequest request); 
}