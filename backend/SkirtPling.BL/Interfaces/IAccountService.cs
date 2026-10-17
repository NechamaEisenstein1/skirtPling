using SkirtPling.BL.DTOs;

namespace SkirtPling.BL.Interfaces;

public interface IAccountService
{
    Task<UserDto> RegisterAsync(RegisterDto dto, CancellationToken ct);

    /// <summary>Checks the password and lockout state. Throws BusinessRuleException with a generic message on failure.</summary>
    Task<UserDto> ValidateCredentialsAsync(LoginDto dto, CancellationToken ct);

    Task<ProfileDto> GetProfileAsync(string userId, CancellationToken ct);
    Task<ProfileDto> UpdateProfileAsync(string userId, UpdateProfileDto dto, CancellationToken ct);
}
