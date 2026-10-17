using Microsoft.AspNetCore.Identity;
using SkirtPling.BL.DTOs;
using SkirtPling.BL.Exceptions;
using SkirtPling.BL.Interfaces;
using SkirtPling.DAL.Entities;
using SkirtPling.DAL.Seed;

namespace SkirtPling.BL.Services;

public class AccountService(UserManager<AppUser> userManager, TimeProvider clock) : IAccountService
{
    private const string InvalidCredentialsMessage = "Invalid email or password.";

    public async Task<UserDto> RegisterAsync(RegisterDto dto, CancellationToken ct)
    {
        var email = dto.Email.Trim();
        if (await userManager.FindByEmailAsync(email) is not null)
            throw new ConflictException("An account with this email already exists.");

        var user = new AppUser
        {
            UserName = email,
            Email = email,
            FullName = dto.FullName.Trim(),
            CreatedAt = clock.GetUtcNow().UtcDateTime
        };

        var result = await userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            throw new BusinessRuleException(string.Join(" ", result.Errors.Select(e => e.Description)));

        await userManager.AddToRoleAsync(user, Roles.Customer);
        return await ToUserDtoAsync(user);
    }

    public async Task<UserDto> ValidateCredentialsAsync(LoginDto dto, CancellationToken ct)
    {
        var user = await userManager.FindByEmailAsync(dto.Email.Trim());
        if (user is null)
            throw new BusinessRuleException(InvalidCredentialsMessage);

        if (await userManager.IsLockedOutAsync(user))
            throw new BusinessRuleException("This account is locked. Please try again later or contact support.");

        if (!await userManager.CheckPasswordAsync(user, dto.Password))
        {
            // Counts failed attempts; Identity locks the account after the configured number of failures.
            await userManager.AccessFailedAsync(user);
            throw new BusinessRuleException(InvalidCredentialsMessage);
        }

        await userManager.ResetAccessFailedCountAsync(user);
        return await ToUserDtoAsync(user);
    }

    public async Task<ProfileDto> GetProfileAsync(string userId, CancellationToken ct)
    {
        var user = await FindUserAsync(userId);
        return ToProfileDto(user);
    }

    public async Task<ProfileDto> UpdateProfileAsync(string userId, UpdateProfileDto dto, CancellationToken ct)
    {
        var user = await FindUserAsync(userId);
        user.FullName = dto.FullName.Trim();
        user.PhoneNumber = string.IsNullOrWhiteSpace(dto.PhoneNumber) ? null : dto.PhoneNumber.Trim();

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
            throw new BusinessRuleException(string.Join(" ", result.Errors.Select(e => e.Description)));
        return ToProfileDto(user);
    }

    private async Task<AppUser> FindUserAsync(string userId) =>
        await userManager.FindByIdAsync(userId) ?? throw new NotFoundException("User not found.");

    private async Task<UserDto> ToUserDtoAsync(AppUser user) =>
        new(user.Id, user.Email!, user.FullName, (await userManager.GetRolesAsync(user)).ToList());

    private static ProfileDto ToProfileDto(AppUser user) => new(user.Id, user.Email!, user.FullName, user.PhoneNumber);
}
