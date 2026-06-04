using Telemetry.Application.Abstractions;
using Telemetry.Application.Models;

namespace Telemetry.Application.Services;

public class TelemetryService(ITelemetryRepository repository) : ITelemetryService
{
    public async Task<IReadOnlyList<MachineSummaryDto>> GetMachinesAsync(CancellationToken ct = default)
    {
        var machineIds = await repository.GetMachineIdsAsync(ct);
        var summaries = new List<MachineSummaryDto>();

        foreach (var id in machineIds)
        {
            var latest = await repository.GetLatestReadingAsync(id, ct);
            if (latest is null) continue;

            var count = await repository.CountByMachineAsync(id, ct);
            summaries.Add(new MachineSummaryDto(
                id, latest.Status, latest.TemperatureC, latest.Rpm, latest.TimestampUtc, count));
        }

        return summaries;
    }

    public async Task<IReadOnlyList<ReadingDto>?> GetReadingsAsync(
        string machineId, int limit, CancellationToken ct = default)
    {
        // Maszyna jest "znana", jeśli ma jakikolwiek odczyt.
        var latest = await repository.GetLatestReadingAsync(machineId, ct);
        if (latest is null) return null;

        limit = Math.Clamp(limit, 1, 500);
        var readings = await repository.GetRecentReadingsAsync(machineId, limit, ct);
        return readings.Select(ReadingDto.FromEntity).ToList();
    }
}
