using Telemetry.Domain;
using Telemetry.Infrastructure.Analysis;

namespace Telemetry.Tests;

public class HeuristicTelemetryAnalyzerTests
{
    private readonly HeuristicTelemetryAnalyzer _sut = new();

    private static TelemetryReading Reading(double temp, int rpm, MachineStatus status) =>
        new() { MachineId = "M-01", TemperatureC = temp, Rpm = rpm, Status = status, TimestampUtc = DateTime.UtcNow };

    [Fact]
    public async Task Normal_conditions_return_Normal_severity()
    {
        var result = await _sut.AnalyzeAsync("M-01", [Reading(60, 1500, MachineStatus.Running)]);

        Assert.Equal("Normal", result.Severity);
        Assert.Equal("heuristic", result.Source);
    }

    [Fact]
    public async Task Temperature_over_100_is_Critical()
    {
        var result = await _sut.AnalyzeAsync("M-01", [Reading(105, 500, MachineStatus.Fault)]);

        Assert.Equal("Critical", result.Severity);
        Assert.Contains("przegrzanie", result.Summary, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task High_temp_with_low_rpm_signals_seizure_and_is_Critical()
    {
        var result = await _sut.AnalyzeAsync("M-01", [Reading(90, 400, MachineStatus.Warning)]);

        Assert.Equal("Critical", result.Severity);
        Assert.Contains("zatarcie", result.Summary, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task High_temp_with_normal_rpm_is_only_Warning()
    {
        var result = await _sut.AnalyzeAsync("M-01", [Reading(88, 1500, MachineStatus.Warning)]);

        Assert.Equal("Warning", result.Severity);
    }

    [Fact]
    public async Task Repeated_anomalies_are_noted_in_summary()
    {
        var readings = new List<TelemetryReading>
        {
            Reading(105, 500, MachineStatus.Fault),
            Reading(98, 400, MachineStatus.Warning),
            Reading(96, 420, MachineStatus.Warning)
        };

        var result = await _sut.AnalyzeAsync("M-01", readings);

        Assert.Equal("Critical", result.Severity);
        Assert.Contains("powtarzalny", result.Summary);
    }
}
