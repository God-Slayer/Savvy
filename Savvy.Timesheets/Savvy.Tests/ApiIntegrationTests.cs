using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Savvy.Infrastructure.Persistence;

namespace Savvy.Tests;

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName =
        $"Data Source=integration-{Guid.NewGuid():N};Mode=Memory;Cache=Shared";
    private Microsoft.Data.Sqlite.SqliteConnection? _connection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:TimesheetsDatabase", "Data Source=integration.db");
        builder.UseSetting("Jwt:Key", "integration-test-key-at-least-32-characters-long");
        builder.UseSetting("Jwt:Issuer", "Savvy.Timesheets");
        builder.UseSetting("Jwt:Audience", "Savvy.Timesheets.Client");
        builder.UseSetting("Jwt:ExpiryMinutes", "60");
        builder.ConfigureServices(services =>
        {
            var descriptor = services.Single(d =>
                d.ServiceType == typeof(DbContextOptions<TimesheetsDbContext>)
            );
            services.Remove(descriptor);
            _connection = new Microsoft.Data.Sqlite.SqliteConnection(_dbName);
            _connection.Open();
            services.AddDbContext<TimesheetsDbContext>(o => o.UseSqlite(_connection));
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _connection?.Dispose();
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);
        using var scope = host.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<TimesheetsDbContext>().Database.EnsureCreated();
        return host;
    }
}

public sealed class ApiIntegrationTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Protected_endpoint_without_token_returns_unauthorized()
    {
        var response = await _client.GetAsync("/api/reports/summary");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_rejects_unknown_credentials()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new { email = "nobody@example.test", password = "invalid" }
        );
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Shift_route_requires_authentication()
    {
        var response = await _client.GetAsync($"/api/practices/{Guid.NewGuid()}/shifts");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Timesheet_route_requires_authentication()
    {
        var response = await _client.GetAsync($"/api/timesheets/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Payment_route_requires_authentication()
    {
        var response = await _client.GetAsync($"/api/payment-runs/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
