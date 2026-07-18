using Mapster;
using Microsoft.Extensions.DependencyInjection;
using Savvy.Application.Shifts;
using Savvy.Domain;

namespace Savvy.Application.Mapping;

/// <summary>Defines mappings between domain entities and API response contracts.</summary>
public static class MapsterConfiguration
{
    public static IServiceCollection AddApplicationMappings(this IServiceCollection services)
    {
        var config = TypeAdapterConfig.GlobalSettings;
        config
            .NewConfig<Shift, ShiftResponseDto>()
            .Map(d => d.Status, s => s.Status.ToString())
            .Map(
                d => d.RowVersion,
                s => Convert.ToBase64String(s.RowVersion ?? Array.Empty<byte>())
            );
        services.AddSingleton(config);
        return services;
    }
}
