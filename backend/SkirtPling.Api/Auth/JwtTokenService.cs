using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SkirtPling.BL.DTOs;

namespace SkirtPling.Api.Auth;

/// <summary>Issues the signed JWT the Angular app sends in the Authorization header.</summary>
public class JwtTokenService(IOptions<JwtOptions> options, TimeProvider clock)
{
    private readonly JwtOptions settings = options.Value;

    public AuthResultDto CreateToken(UserDto user)
    {
        var expiresAt = clock.GetUtcNow().UtcDateTime.AddMinutes(settings.ExpiryMinutes);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Name, user.FullName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Key));
        var token = new JwtSecurityToken(
            issuer: settings.Issuer,
            audience: settings.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256));

        return new AuthResultDto(new JwtSecurityTokenHandler().WriteToken(token), expiresAt, user);
    }
}
