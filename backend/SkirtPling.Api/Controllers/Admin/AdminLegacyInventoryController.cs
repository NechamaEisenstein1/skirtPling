using Microsoft.AspNetCore.Mvc;
using SkirtPling.BL.DTOs;
using SkirtPling.BL.Interfaces;

namespace SkirtPling.Api.Controllers.Admin;

/// <summary>Stock reconciliation against the legacy .NET Framework warehouse service (WCF).</summary>
[Route("api/admin/legacy-inventory")]
public class AdminLegacyInventoryController(ILegacyInventoryService legacy) : AdminControllerBase
{
    [HttpGet("status")]
    public object Status() => new { legacy.IsAvailable };

    [HttpGet("{sku}")]
    public async Task<ActionResult<LegacyStockDto>> Compare(string sku, CancellationToken ct)
    {
        if (!legacy.IsAvailable)
            return Problem(statusCode: StatusCodes.Status503ServiceUnavailable, title: "Legacy warehouse is not configured",
                detail: "Set LegacyWarehouse:Endpoint and start the WCF service (see docs/legacy-integration.md).");

        return await legacy.CompareStockAsync(sku, ct);
    }
}
