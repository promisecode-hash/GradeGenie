using GradeGenie.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace GradeGenie.Api.Tests;

public sealed class GradeGenieApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"gradegenie-api-{Guid.NewGuid():N}.db");

    public GradeGenieApiFactory()
    {
        Environment.SetEnvironmentVariable("Jwt__Key", "integration-test-signing-key-that-is-long-enough");
        Environment.SetEnvironmentVariable("ConnectionStrings__GradeGenie", $"Data Source={_databasePath}");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Production");
        builder.ConfigureLogging(logging => logging.ClearProviders());
        builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:GradeGenie"] = $"Data Source={_databasePath}",
            ["Jwt:Key"] = "integration-test-signing-key-that-is-long-enough"
        }));
    }

    public async Task InitializeDatabaseAsync()
    {
        await using var scope = Services.CreateAsyncScope();
        var database = scope.ServiceProvider.GetRequiredService<GradeGenieDbContext>();
        if (database.Database.ProviderName?.Contains("Sqlite", StringComparison.OrdinalIgnoreCase) == true)
        {
            await database.Database.EnsureCreatedAsync();
        }
        else
        {
            await database.Database.MigrateAsync();
        }
    }

}
