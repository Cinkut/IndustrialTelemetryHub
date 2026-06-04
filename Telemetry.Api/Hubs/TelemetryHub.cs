using Microsoft.AspNetCore.SignalR;

namespace Telemetry.Api.Hubs;

/// <summary>
/// Hub SignalR przesyłający telemetrię w czasie rzeczywistym do przeglądarek.
/// Serwer wypycha wiadomości "ReadingReceived"; klient nie musi nic wywoływać.
/// </summary>
public class TelemetryHub : Hub
{
}
