using Microsoft.EntityFrameworkCore;
using Telemetry.Domain;

namespace Telemetry.Infrastructure.Data;

public class TelemetryDbContext(DbContextOptions<TelemetryDbContext> options) : DbContext(options)
{
    public DbSet<TelemetryReading> Readings => Set<TelemetryReading>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TelemetryReading>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.MachineId).IsRequired().HasMaxLength(50);
            entity.Property(r => r.Status).HasConversion<string>();

            // Indeks pod zapytania time-series: ostatnie odczyty danej maszyny.
            entity.HasIndex(r => new { r.MachineId, r.TimestampUtc });
        });
    }
}
