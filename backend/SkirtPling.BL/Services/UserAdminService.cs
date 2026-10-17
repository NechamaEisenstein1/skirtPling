using Microsoft.AspNetCore.Identity;
using SkirtPling.BL.DTOs;
using SkirtPling.BL.Exceptions;
using SkirtPling.BL.Interfaces;
using SkirtPling.DAL.Entities;
using SkirtPling.DAL.Repositories;
using SkirtPling.DAL.Seed;

namespace SkirtPling.BL.Services;

public class UserAdminService(UserManager<AppUser> userManager, IUserRepository users, TimeProvider clock) : IUserAdminService
{
    public async Task<PagedResultDto<AdminUserDto>> SearchAsync(string? search, int page, int pageSize, CancellationToken ct)
    {
        var result = await users.SearchAsync(search, page, pageSize, ct);

        var items = new List<AdminUserDto>();
        foreach (var user in result.Items)
            items.Add(await ToDtoAsync(user));

        return new PagedResultDto<AdminUserDto>(items, page, pageSize, result.TotalCount);
    }

    public async Task<AdminUserDto> UpdateAccessAsync(string currentAdminId, string userId, UpdateUserAccessDto dto, CancellationToken ct)
    {
        if (currentAdminId == userId)
            throw new BusinessRuleException("You cannot change your own access. Ask another admin.");

        var user = await userManager.FindByIdAsync(userId) ?? throw new NotFoundException("User not found.");

        var isAdmin = await userManager.IsInRoleAsync(user, Roles.Admin);
        if (dto.IsAdmin && !isAdmin)
            await userManager.AddToRoleAsync(user, Roles.Admin);
        else if (!dto.IsAdmin && isAdmin)
            await userManager.RemoveFromRoleAsync(user, Roles.Admin);

        await userManager.SetLockoutEnabledAsync(user, true);
        await userManager.SetLockoutEndDateAsync(user, dto.IsLocked ? DateTimeOffset.MaxValue : null);

        return await ToDtoAsync(user);
    }

    private async Task<AdminUserDto> ToDtoAsync(AppUser user) => new(
        user.Id, user.Email!, user.FullName,
        (await userManager.GetRolesAsync(user)).ToList(),
        user.LockoutEnd is not null && user.LockoutEnd > clock.GetUtcNow(),
        user.CreatedAt);
}
