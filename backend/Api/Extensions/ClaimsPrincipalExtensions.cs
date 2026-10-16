using System.Security.Claims;
using Infrastructure.Services;

namespace Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Reads the authenticated user's id from the "sub" claim of the access token.
    /// </summary>
    public static int GetUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(JwtClaimNames.Subject);

        if (int.TryParse(value, out var userId))
            return userId;

        throw new UnauthorizedAccessException(
            "The access token does not contain a valid user identifier.");
    }
}
