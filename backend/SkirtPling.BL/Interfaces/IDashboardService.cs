using SkirtPling.BL.DTOs;

namespace SkirtPling.BL.Interfaces;

public interface IDashboardService
{
    Task<DashboardStatsDto> GetStatsAsync(CancellationToken ct);
}
