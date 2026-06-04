using Microsoft.AspNetCore.Mvc;
using Telemetry.Application.Models;
using Telemetry.Application.Services;

namespace Telemetry.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MachinesController(ITelemetryService service) : ControllerBase
{
    /// <summary>Lista maszyn z ich najnowszym stanem.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MachineSummaryDto>>> GetMachines(CancellationToken ct)
        => Ok(await service.GetMachinesAsync(ct));

    /// <summary>Ostatnie odczyty telemetrii danej maszyny.</summary>
    [HttpGet("{machineId}/readings")]
    public async Task<ActionResult<IReadOnlyList<ReadingDto>>> GetReadings(
        string machineId, CancellationToken ct, [FromQuery] int limit = 50)
    {
        var readings = await service.GetReadingsAsync(machineId, limit, ct);
        return readings is null ? NotFound($"Brak danych dla maszyny '{machineId}'.") : Ok(readings);
    }
}
