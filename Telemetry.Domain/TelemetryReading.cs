namespace Telemetry.Domain;

/// <summary>
/// Pojedynczy odczyt telemetrii z maszyny. Pełni rolę kontraktu wiadomości MQTT
/// (serializowany do JSON) oraz — od Etapu 2 — encji zapisywanej w bazie.
/// </summary>
public class TelemetryReading
{
    /// <summary>Identyfikator maszyny, np. "M-01".</summary>
    public string MachineId { get; set; } = string.Empty;

    /// <summary>Czas pomiaru (UTC).</summary>
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;

    /// <summary>Temperatura w stopniach Celsjusza.</summary>
    public double TemperatureC { get; set; }

    /// <summary>Obroty na minutę.</summary>
    public int Rpm { get; set; }

    public MachineStatus Status { get; set; } = MachineStatus.Idle;
}
