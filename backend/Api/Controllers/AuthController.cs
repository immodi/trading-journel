using Api.Extensions;
using Application.Interfaces.Services;
using Application.Requests.Auth;
using Application.Responses;
using Application.Responses.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService, IUserService userService) : ControllerBase
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
        var result = await authService.RegisterAsync(request);

        if (!result.IsSuccess)
        {
            var message = result.Outcome switch
            {
                RegisterOutcome.EmailTaken =>
                    "An account with that email already exists.",
                RegisterOutcome.UsernameTaken =>
                    "That username is already taken.",
                _ => "An account with those details already exists."
            };

            return Conflict(new ErrorResponse(409, message));
        }

        var token = await authService.LoginAsync(
            new LoginRequest { Email = request.Email, Password = request.Password });

        if (token is null)
            return Unauthorized(
                new ErrorResponse(401, "Error occured while logging in the user"));

        return Ok(new AuthResponse
        {
            Token = token
        });
    }

    /// <summary>
    /// Returns the profile of the user the access token belongs to.
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var user = await userService.GetByIdAsync(User.GetUserId());

        if (user is null)
            return NotFound(new ErrorResponse(404, "User not found."));

        return Ok(user);
    }
}
