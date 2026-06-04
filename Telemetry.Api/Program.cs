using Microsoft.EntityFrameworkCore;
using Telemetry.Api.Ingestion;
using Telemetry.Infrastructure;
using Telemetry.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Warstwa danych (EF Core + PostgreSQL).
builder.Services.AddInfrastructure(builder.Configuration);

// Usługa w tle: odbiór telemetrii z MQTT i zapis do bazy.
builder.Services.AddHostedService<MqttIngestionService>();

builder.Services.AddControllers();

var app = builder.Build();

// Automatyczne migracje przy starcie.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TelemetryDbContext>();
    db.Database.Migrate();
}

app.UseAuthorization();
app.MapControllers();

app.Run();
