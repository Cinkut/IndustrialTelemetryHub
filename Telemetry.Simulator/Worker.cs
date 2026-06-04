using System.Text.Json;
using MQTTnet;
using Telemetry.Domain;

namespace Telemetry.Simulator;

/// <summary>
/// Symuluje park maszyn przemysłowych: co kilka sekund publikuje odczyty
/// telemetrii do brokera MQTT (temat: factory/{machineId}/telemetry).
/// Sporadycznie generuje anomalie (przegrzanie) do testów detekcji.
/// </summary>
public class Worker(ILogger<Worker> logger, IConfiguration config) : BackgroundService
{
    private static readonly string[] Machines = ["M-01", "M-02", "M-03"];
    private readonly Random _rng = new();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var host = config.GetValue<string>("Mqtt:Host") ?? "localhost";
        var port = config.GetValue<int?>("Mqtt:Port") ?? 1883;
        var interval = TimeSpan.FromSeconds(config.GetValue<int?>("Mqtt:IntervalSeconds") ?? 2);

        var factory = new MqttClientFactory();
        using var client = factory.CreateMqttClient();
        var options = new MqttClientOptionsBuilder()
            .WithTcpServer(host, port)
            .WithClientId($"simulator-{Guid.NewGuid():N}")
            .Build();

        await ConnectWithRetryAsync(client, options, stoppingToken);
        logger.LogInformation("Symulator połączony z brokerem MQTT {Host}:{Port}", host, port);

        while (!stoppingToken.IsCancellationRequested)
        {
            foreach (var machineId in Machines)
            {
                var reading = GenerateReading(machineId);
                var payload = JsonSerializer.SerializeToUtf8Bytes(reading);

                var message = new MqttApplicationMessageBuilder()
                    .WithTopic($"factory/{machineId}/telemetry")
                    .WithPayload(payload)
                    .Build();

                if (!client.IsConnected)
                    await ConnectWithRetryAsync(client, options, stoppingToken);

                await client.PublishAsync(message, stoppingToken);
                logger.LogInformation("{Machine}: {Temp:F1}°C, {Rpm} RPM, {Status}",
                    reading.MachineId, reading.TemperatureC, reading.Rpm, reading.Status);
            }

            await Task.Delay(interval, stoppingToken);
        }
    }

    private TelemetryReading GenerateReading(string machineId)
    {
        // 10% szans na anomalię (przegrzanie) — przyda się do detekcji w warstwie AI.
        var isAnomaly = _rng.NextDouble() < 0.10;
        var temperature = isAnomaly
            ? 90 + _rng.NextDouble() * 20      // 90–110°C (przegrzanie)
            : 55 + _rng.NextDouble() * 15;     // 55–70°C (norma)

        var rpm = isAnomaly ? _rng.Next(200, 600) : _rng.Next(1400, 1600);

        var status = temperature switch
        {
            >= 100 => MachineStatus.Fault,
            >= 85 => MachineStatus.Warning,
            _ => MachineStatus.Running
        };

        return new TelemetryReading
        {
            MachineId = machineId,
            TimestampUtc = DateTime.UtcNow,
            TemperatureC = Math.Round(temperature, 1),
            Rpm = rpm,
            Status = status
        };
    }

    private async Task ConnectWithRetryAsync(
        IMqttClient client, MqttClientOptions options, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                await client.ConnectAsync(options, ct);
                return;
            }
            catch (Exception ex)
            {
                logger.LogWarning("Broker MQTT niedostępny ({Message}). Ponawiam za 3s...", ex.Message);
                await Task.Delay(TimeSpan.FromSeconds(3), ct);
            }
        }
    }
}
