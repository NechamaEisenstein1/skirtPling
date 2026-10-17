using SkirtPling.BL.DTOs;

namespace SkirtPling.BL.Interfaces;

public interface IUserAdminService
{
    Task<PagedResultDto<AdminUserDto>> SearchAsync(string? search, int page, int pageSize, CancellationToken ct);

    /// <summary>Grants/revokes admin and locks/unlocks a user. Admins cannot change their own access.</summary>
    Task<AdminUserDto> UpdateAccessAsync(string currentAdminId, string userId, UpdateUserAccessDto dto, CancellationToken ct);
}
