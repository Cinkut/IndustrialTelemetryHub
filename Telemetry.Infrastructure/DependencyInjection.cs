using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Telemetry.Application.Abstractions;
using Telemetry.Infrastructure.Data;
using Telemetry.Infrastructure.Repositories;

namespace Telemetry.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<TelemetryDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Default")));

        services.AddScoped<ITelemetryRepository, TelemetryRepository>();
        return services;
    }
}
