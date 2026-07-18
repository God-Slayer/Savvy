using Savvy.Application.Authentication;
using Savvy.Application.Mapping;
using Savvy.Application.PaymentRuns;
using Savvy.Application.Shifts;
using Savvy.Application.Timesheets;

namespace Savvy.Api.Configuration;

/// <summary>Registers API framework services, controllers, and shared infrastructure behavior.</summary>
public static class ApiServiceConfiguration
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IShiftService, ShiftService>();
        services.AddScoped<ITimesheetService, TimesheetService>();
        services.AddScoped<IPaymentRunService, PaymentRunService>();
        services.AddApplicationMappings();
        return services;
    }
}
