using Telemetry.Application.Abstractions;
using Telemetry.Application.Models;
using Telemetry.Domain;

namespace Telemetry.Infrastructure.Analysis;

/// <summary>
/// Analiza oparta o ręczne reguły (heurystyka). Działa zawsze — bez klucza API
/// i bez dostępu do internetu. Stanowi też fallback dla wariantu LLM.
/// </summary>
public class HeuristicTelemetryAnalyzer : ITelemetryAnalyzer
{
    public Task<MachineAnalysis> AnalyzeAsync(
        string machineId, IReadOnlyList<TelemetryReading> recent, CancellationToken ct = default)
    {
        var latest = recent[0]; // lista posortowana malejąco po czasie
        var warningCount = recent.Count(r => r.Status is MachineStatus.Warning or MachineStatus.Fault);

        string severity;
        string summary;

        if (latest.TemperatureC >= 100)
        {
            severity = "Critical";
            summary = $"Krytyczne przegrzanie maszyny {machineId}: {latest.TemperatureC:F1}°C przy " +
                      $"{latest.Rpm} RPM. Ryzyko awarii — zaleca się natychmiastowe zatrzymanie.";
        }
        else if (latest.TemperatureC >= 85 && latest.Rpm < 800)
        {
            severity = "Critical";
            summary = $"Maszyna {machineId} przegrzewa się ({latest.TemperatureC:F1}°C) przy spadku obrotów " +
                      $"do {latest.Rpm} RPM — prawdopodobne zatarcie. Zalecany pilny przegląd.";
        }
        else if (latest.TemperatureC >= 85)
        {
            severity = "Warning";
            summary = $"Podwyższona temperatura maszyny {machineId}: {latest.TemperatureC:F1}°C. " +
                      "Warto monitorować i sprawdzić układ chłodzenia.";
        }
        else
        {
            severity = "Normal";
            summary = $"Maszyna {machineId} pracuje prawidłowo ({latest.TemperatureC:F1}°C, {latest.Rpm} RPM).";
        }

        if (warningCount > 1 && severity != "Normal")
            summary += $" W ostatnich {recent.Count} odczytach wystąpiło {warningCount} ostrzeżeń — problem może być powtarzalny.";

        return Task.FromResult(new MachineAnalysis(machineId, severity, summary, "heuristic"));
    }
}
