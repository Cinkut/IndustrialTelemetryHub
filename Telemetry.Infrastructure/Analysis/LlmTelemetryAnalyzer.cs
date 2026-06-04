using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Telemetry.Application.Abstractions;
using Telemetry.Application.Models;
using Telemetry.Domain;

namespace Telemetry.Infrastructure.Analysis;

/// <summary>
/// Analiza oparta o model językowy (Anthropic Messages API). Używana, gdy w konfiguracji
/// ustawiono klucz "Ai:ApiKey". Przy dowolnym błędzie spada na heurystykę (graceful degradation).
/// </summary>
public class LlmTelemetryAnalyzer(HttpClient http, IConfiguration config, ILogger<LlmTelemetryAnalyzer> logger)
    : ITelemetryAnalyzer
{
    private readonly HeuristicTelemetryAnalyzer _fallback = new();

    public async Task<MachineAnalysis> AnalyzeAsync(
        string machineId, IReadOnlyList<TelemetryReading> recent, CancellationToken ct = default)
    {
        try
        {
            var data = string.Join("\n", recent.Take(20).Select(r =>
                $"{r.TimestampUtc:HH:mm:ss}: {r.TemperatureC:F1}°C, {r.Rpm} RPM, {r.Status}"));

            var prompt = $"""
                Jesteś inżynierem utrzymania ruchu. Na podstawie poniższej telemetrii maszyny {machineId}
                opisz w 1-2 zdaniach po polsku jej stan i ewentualne zagrożenia. Bądź konkretny.

                Ostatnie odczyty:
                {data}
                """;

            var request = new
            {
                model = config["Ai:Model"] ?? "claude-sonnet-4-5",
                max_tokens = 200,
                messages = new[] { new { role = "user", content = prompt } }
            };

            using var msg = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages");
            msg.Headers.Add("x-api-key", config["Ai:ApiKey"]);
            msg.Headers.Add("anthropic-version", "2023-06-01");
            msg.Content = JsonContent.Create(request);

            var res = await http.SendAsync(msg, ct);
            res.EnsureSuccessStatusCode();

            var json = await res.Content.ReadFromJsonAsync<JsonElement>(ct);
            var text = json.GetProperty("content")[0].GetProperty("text").GetString();

            var severity = (await _fallback.AnalyzeAsync(machineId, recent, ct)).Severity;
            return new MachineAnalysis(machineId, severity, text?.Trim() ?? "", "llm");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Analiza LLM nie powiodła się — fallback na heurystykę.");
            return await _fallback.AnalyzeAsync(machineId, recent, ct);
        }
    }
}
