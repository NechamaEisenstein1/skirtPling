using Microsoft.AspNetCore.Mvc;
using SkirtPling.BL.DTOs;
using SkirtPling.BL.Interfaces;

namespace SkirtPling.Api.Controllers.Admin;

[Route("api/admin/dashboard")]
public class AdminDashboardController(IDashboardService dashboard) : AdminControllerBase
{
    [HttpGet]
    public Task<DashboardStatsDto> Get(CancellationToken ct) => dashboard.GetStatsAsync(ct);
}
