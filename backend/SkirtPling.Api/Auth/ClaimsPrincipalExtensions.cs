using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SkirtPling.Api.Auth;

public static class ClaimsPrincipalExtensions
{
    /// <summary>The signed-in user's id. Only call this on endpoints that require authentication.</summary>
    public static string GetUserId(this ClaimsPrincipal user) =>
        user.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? throw new InvalidOperationException("The user id claim is missing.");

    public static string GetEmail(this ClaimsPrincipal user) =>
        user.FindFirstValue(JwtRegisteredClaimNames.Email) ?? throw new InvalidOperationException("The email claim is missing.");
}
