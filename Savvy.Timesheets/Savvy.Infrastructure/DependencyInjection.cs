using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Savvy.Application.Authentication;
using Savvy.Application.Persistence;
using Savvy.Application.Reporting;
using Savvy.Domain;
using Savvy.Infrastructure.Authentication;
using Savvy.Infrastructure.Development;
using Savvy.Infrastructure.Persistence;
using Savvy.Infrastructure.Persistence.Repositories;
using Savvy.Infrastructure.Reporting;

namespace Savvy.Infrastructure;

/// <summary>Composes Infrastructure dependencies such as EF Core, repositories, authentication, and seeding.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var connectionString =
            configuration.GetConnectionString("TimesheetsDatabase")
            ?? throw new InvalidOperationException(
                "Connection string 'TimesheetsDatabase' is not configured."
            );

        var jwtSection = configuration.GetSection(JwtOptions.SectionName);
        var jwtOptions = new JwtOptions
        {
            Key = jwtSection["Key"] ?? string.Empty,
            Issuer = jwtSection["Issuer"] ?? string.Empty,
            Audience = jwtSection["Audience"] ?? string.Empty,
            ExpiryMinutes = int.TryParse(jwtSection["ExpiryMinutes"], out var expiryMinutes)
                ? expiryMinutes
                : 0,
        };
        services
            .AddOptions<JwtOptions>()
            .Configure(options =>
            {
                options.Key = jwtOptions.Key;
                options.Issuer = jwtOptions.Issuer;
                options.Audience = jwtOptions.Audience;
                options.ExpiryMinutes = jwtOptions.ExpiryMinutes;
            })
            .Validate(
                options =>
                    !string.IsNullOrWhiteSpace(options.Key)
                    && options.Key.Length >= 32
                    && !string.IsNullOrWhiteSpace(options.Issuer)
                    && !string.IsNullOrWhiteSpace(options.Audience)
                    && options.ExpiryMinutes is >= 1 and <= 1440,
                "JWT configuration is invalid."
            )
            .ValidateOnStart();

        return services.AddInfrastructure(connectionString);
    }

    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString
    )
    {
        services.AddDbContext<TimesheetsDbContext>(options =>
            options.UseSqlServer(connectionString)
        );
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IShiftRepository, ShiftRepository>();
        services.AddScoped<ITimesheetRepository, TimesheetRepository>();
        services.AddScoped<IPaymentRunRepository, PaymentRunRepository>();
        services.AddScoped<IReportingService, ReportingService>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddSingleton<IAccessTokenGenerator, JwtAccessTokenGenerator>();
        services.AddScoped<DevelopmentDataSeeder>();
        return services;
    }
}
