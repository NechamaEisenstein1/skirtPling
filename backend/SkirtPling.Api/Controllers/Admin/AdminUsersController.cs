using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using SkirtPling.Api.Auth;
using SkirtPling.BL.DTOs;
using SkirtPling.BL.Interfaces;

namespace SkirtPling.Api.Controllers.Admin;

[Route("api/admin/users")]
public class AdminUsersController(IUserAdminService users) : AdminControllerBase
{
    [HttpGet]
    public Task<PagedResultDto<AdminUserDto>> Search(
        [FromQuery, StringLength(100)] string? search,
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 20,
        CancellationToken ct = default) =>
        users.SearchAsync(search, page, pageSize, ct);

    [HttpPut("{id}/access")]
    public Task<AdminUserDto> UpdateAccess(string id, UpdateUserAccessDto dto, CancellationToken ct) =>
        users.UpdateAccessAsync(User.GetUserId(), id, dto, ct);
}
