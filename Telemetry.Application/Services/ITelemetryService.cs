using Telemetry.Application.Models;

namespace Telemetry.Application.Services;

public interface ITelemetryService
{
    /// <summary>Podsumowanie wszystkich maszyn (najnowszy odczyt każdej).</summary>
    Task<IReadOnlyList<MachineSummaryDto>> GetMachinesAsync(CancellationToken ct = default);

    /// <summary>Ostatnie odczyty danej maszyny. Zwraca null, gdy maszyna nieznana.</summary>
    Task<IReadOnlyList<ReadingDto>?> GetReadingsAsync(
        string machineId, int limit, CancellationToken ct = default);
}
