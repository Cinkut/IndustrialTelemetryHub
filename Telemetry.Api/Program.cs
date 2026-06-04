using Microsoft.EntityFrameworkCore;
using Telemetry.Api.Ingestion;
using Telemetry.Application;
using Telemetry.Infrastructure;
using Telemetry.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Warstwy aplikacji.
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

// Usługa w tle: odbiór telemetrii z MQTT i zapis do bazy.
builder.Services.AddHostedService<MqttIngestionService>();

// Kontrolery + Swagger.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Automatyczne migracje przy starcie.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TelemetryDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

app.Run();
