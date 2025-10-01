using System.Security.Claims;
using service.src.models;

namespace service.src.services;

public class JwtTokenService(IHttpContextAccessor httpContextAccessor)
{
    /// <summary>
    /// Gets the authenticated user details from the current HttpContext.
    /// </summary>
    /// <returns>A User object if authenticated, otherwise null.</returns>
    public User? GetCurrentUser()
    {
        var principal = httpContextAccessor.HttpContext?.User;
        if (principal == null)
        {
            return null;
        }

        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = principal.FindFirstValue(ClaimTypes.Email);

        if (userId is null || email is null)
        {
            return null;
        }

        return new User
        {
            Id = Guid.Parse(userId),
            Email = email,
            PasswordHash = ""
        };
    }
}
