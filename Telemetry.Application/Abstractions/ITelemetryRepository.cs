using Telemetry.Domain;

namespace Telemetry.Application.Abstractions;

/// <summary>Abstrakcja dostępu do zapisanej telemetrii (niezależna od EF Core).</summary>
public interface ITelemetryRepository
{
    Task<IReadOnlyList<string>> GetMachineIdsAsync(CancellationToken ct = default);
    Task<TelemetryReading?> GetLatestReadingAsync(string machineId, CancellationToken ct = default);
    Task<int> CountByMachineAsync(string machineId, CancellationToken ct = default);
    Task<IReadOnlyList<TelemetryReading>> GetRecentReadingsAsync(
        string machineId, int limit, CancellationToken ct = default);
}
