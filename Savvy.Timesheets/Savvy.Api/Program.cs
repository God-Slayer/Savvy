using Microsoft.EntityFrameworkCore;
using Savvy.Api.Configuration;
using Savvy.Api.Controllers;
using Savvy.Infrastructure;
using Savvy.Infrastructure.Development;
using Savvy.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApiServices();
builder.Services.AddApiAuthentication(builder.Configuration);
builder.Services.AddApiSwagger();

builder.Services.AddControllers().AddApplicationPart(typeof(ControllersAssemblyMarker).Assembly);

var app = builder.Build();
app.UseMiddleware<Savvy.Api.OperationalMiddleware>();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<TimesheetsDbContext>();
    await dbContext.Database.MigrateAsync();
    await scope.ServiceProvider.GetRequiredService<DevelopmentDataSeeder>().SeedAsync();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
