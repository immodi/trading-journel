using Application.Interfaces.Services;
using Application.Requests.Auth;
using Application.Responses;
using Application.Responses.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var token = await authService.LoginAsync(request);

        if (token is null)
            return Unauthorized(
                new ErrorResponse(401, "Invalid email or password."));

        return Ok(new AuthResponse
        {
            Token = token
        });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var user = await authService.RegisterAsync(request);

        if (user is null)
            return Unauthorized(
                new ErrorResponse(401, "An error occured while registering the user"));
        
        var token = await authService.LoginAsync(
            new LoginRequest{Email = request.Email, Password = request.Password});

        if (token is null)
            return Unauthorized(
                new ErrorResponse(401, "Error occured while logging in the user"));

        return Ok(new AuthResponse
        {
            Token = token
        });

    }
}