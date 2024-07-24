using System.Security.Claims;

namespace Fei.Is.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static string GetUserId(this ClaimsPrincipal claims)
    {
        var userId = claims.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            throw new InvalidOperationException("User ID claim is missing.");
        }

        return userId;
    }

    public static string GetEmail(this ClaimsPrincipal claims)
    {
        var email = claims.FindFirstValue(ClaimTypes.Email);

        if (email == null)
        {
            throw new InvalidOperationException("Email claim is missing.");
        }

        return email;
    }

    public static bool IsAdmin(this ClaimsPrincipal claims)
    {
        return claims.IsInRole("Admin");
    }
}
