using System.Buffers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using MQTTnet;
using Telemetry.Api.Hubs;
using Telemetry.Domain;
using Telemetry.Infrastructure.Data;

namespace Telemetry.Api.Ingestion;

/// <summary>
/// Subskrybuje telemetrię z brokera MQTT (factory/#), deserializuje odczyty,
/// zapisuje je do bazy i wypycha na żywo do przeglądarek przez SignalR.
/// Działa jako usługa hostowana w tle.
/// </summary>
public class MqttIngestionService(
    ILogger<MqttIngestionService> logger,
    IConfiguration config,
    IServiceScopeFactory scopeFactory,
    IHubContext<TelemetryHub> hub) : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    private IMqttClient? _client;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var host = config.GetValue<string>("Mqtt:Host") ?? "localhost";
        var port = config.GetValue<int?>("Mqtt:Port") ?? 1883;

        var factory = new MqttClientFactory();
        _client = factory.CreateMqttClient();
        _client.ApplicationMessageReceivedAsync += OnMessageReceivedAsync;

        var options = new MqttClientOptionsBuilder()
            .WithTcpServer(host, port)
            .WithClientId($"ingestion-{Guid.NewGuid():N}")
            .Build();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _client.ConnectAsync(options, stoppingToken);
                await _client.SubscribeAsync("factory/#", cancellationToken: stoppingToken);
                logger.LogInformation("Ingestia połączona z MQTT {Host}:{Port}, subskrybuję factory/#", host, port);
                break;
            }
            catch (Exception ex)
            {
                logger.LogWarning("Broker MQTT niedostępny ({Message}). Ponawiam za 3s...", ex.Message);
                await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
            }
        }

        // Utrzymuj usługę przy życiu — odbiór wiadomości dzieje się w evencie.
        try { await Task.Delay(Timeout.Infinite, stoppingToken); }
        catch (OperationCanceledException) { /* zamykanie aplikacji */ }
    }

    private async Task OnMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
    {
        try
        {
            var bytes = e.ApplicationMessage.Payload.ToArray();
            if (bytes.Length == 0) return;

            var json = Encoding.UTF8.GetString(bytes);
            var reading = JsonSerializer.Deserialize<TelemetryReading>(json, JsonOptions);
            if (reading is null) return;

            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<TelemetryDbContext>();
            db.Readings.Add(reading);
            await db.SaveChangesAsync();

            // Push na żywo do podłączonych przeglądarek (status jako tekst dla czytelności w JS).
            await hub.Clients.All.SendAsync("ReadingReceived", new
            {
                machineId = reading.MachineId,
                status = reading.Status.ToString(),
                temperatureC = reading.TemperatureC,
                rpm = reading.Rpm,
                timestampUtc = reading.TimestampUtc
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Błąd przetwarzania wiadomości MQTT");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_client is { IsConnected: true })
            await _client.DisconnectAsync();
        _client?.Dispose();
        await base.StopAsync(cancellationToken);
    }
}
