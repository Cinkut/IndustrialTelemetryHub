using Microsoft.EntityFrameworkCore;
using Telemetry.Application.Abstractions;
using Telemetry.Domain;
using Telemetry.Infrastructure.Data;

namespace Telemetry.Infrastructure.Repositories;

public class TelemetryRepository(TelemetryDbContext db) : ITelemetryRepository
{
    public async Task<IReadOnlyList<string>> GetMachineIdsAsync(CancellationToken ct = default)
        => await db.Readings.Select(r => r.MachineId).Distinct().OrderBy(id => id).ToListAsync(ct);

    public async Task<TelemetryReading?> GetLatestReadingAsync(string machineId, CancellationToken ct = default)
        => await db.Readings.AsNoTracking()
            .Where(r => r.MachineId == machineId)
            .OrderByDescending(r => r.TimestampUtc)
            .FirstOrDefaultAsync(ct);

    public async Task<int> CountByMachineAsync(string machineId, CancellationToken ct = default)
        => await db.Readings.CountAsync(r => r.MachineId == machineId, ct);

    public async Task<IReadOnlyList<TelemetryReading>> GetRecentReadingsAsync(
        string machineId, int limit, CancellationToken ct = default)
        => await db.Readings.AsNoTracking()
            .Where(r => r.MachineId == machineId)
            .OrderByDescending(r => r.TimestampUtc)
            .Take(limit)
            .ToListAsync(ct);
}
