using Telemetry.Domain;

namespace Telemetry.Application.Models;

/// <summary>Podsumowanie stanu maszyny (najnowszy odczyt + statystyki).</summary>
public record MachineSummaryDto(
    string MachineId,
    MachineStatus Status,
    double TemperatureC,
    int Rpm,
    DateTime LastSeenUtc,
    int TotalReadings);

/// <summary>Pojedynczy odczyt telemetrii zwracany przez API.</summary>
public record ReadingDto(DateTime TimestampUtc, double TemperatureC, int Rpm, MachineStatus Status)
{
    public static ReadingDto FromEntity(TelemetryReading r) =>
        new(r.TimestampUtc, r.TemperatureC, r.Rpm, r.Status);
}
