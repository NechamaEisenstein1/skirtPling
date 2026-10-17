namespace SkirtPling.Api.Auth;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "SkirtPling";
    public string Audience { get; set; } = "SkirtPling.Web";

    /// <summary>HMAC signing key, at least 32 characters. Supply it via user-secrets or an environment variable outside development.</summary>
    public string Key { get; set; } = string.Empty;

    public int ExpiryMinutes { get; set; } = 120;
}
