using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkirtPling.Api.Auth;
using SkirtPling.BL.DTOs;
using SkirtPling.BL.Interfaces;

namespace SkirtPling.Api.Controllers;

/// <summary>The signed-in user's own profile and saved addresses. The user id always comes from the token.</summary>
[ApiController]
[Authorize]
[Route("api/account")]
public class AccountController(IAccountService accounts, IAddressService addresses) : ControllerBase
{
    [HttpGet("profile")]
    public Task<ProfileDto> GetProfile(CancellationToken ct) => accounts.GetProfileAsync(User.GetUserId(), ct);

    [HttpPut("profile")]
    public Task<ProfileDto> UpdateProfile(UpdateProfileDto dto, CancellationToken ct) =>
        accounts.UpdateProfileAsync(User.GetUserId(), dto, ct);

    [HttpGet("addresses")]
    public Task<IReadOnlyList<AddressDto>> GetAddresses(CancellationToken ct) => addresses.GetAllAsync(User.GetUserId(), ct);

    [HttpPost("addresses")]
    public async Task<ActionResult<AddressDto>> CreateAddress(AddressUpsertDto dto, CancellationToken ct)
    {
        var address = await addresses.CreateAsync(User.GetUserId(), dto, ct);
        return StatusCode(StatusCodes.Status201Created, address);
    }

    [HttpPut("addresses/{id:int}")]
    public Task<AddressDto> UpdateAddress(int id, AddressUpsertDto dto, CancellationToken ct) =>
        addresses.UpdateAsync(User.GetUserId(), id, dto, ct);

    [HttpDelete("addresses/{id:int}")]
    public async Task<IActionResult> DeleteAddress(int id, CancellationToken ct)
    {
        await addresses.DeleteAsync(User.GetUserId(), id, ct);
        return NoContent();
    }
}
