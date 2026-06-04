using Telemetry.Application.Models;
using Telemetry.Domain;

namespace Telemetry.Application.Abstractions;

/// <summary>
/// Analizuje stan maszyny na podstawie ostatnich odczytów telemetrii.
/// Implementacja może być heurystyczna (reguły) lub oparta o LLM —
/// reszta aplikacji nie musi wiedzieć która.
/// </summary>
public interface ITelemetryAnalyzer
{
    Task<MachineAnalysis> AnalyzeAsync(
        string machineId, IReadOnlyList<TelemetryReading> recent, CancellationToken ct = default);
}
