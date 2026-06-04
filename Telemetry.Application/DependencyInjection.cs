using Microsoft.Extensions.DependencyInjection;
using Telemetry.Application.Services;

namespace Telemetry.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ITelemetryService, TelemetryService>();
        return services;
    }
}
