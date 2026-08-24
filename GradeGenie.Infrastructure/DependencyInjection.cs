using GradeGenie.Domain.Interfaces;
using GradeGenie.Infrastructure.AI;
using GradeGenie.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GradeGenie.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("GradeGenie");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Connection string 'GradeGenie' is not configured. Set ConnectionStrings:GradeGenie via appsettings, user-secrets or an environment variable (ConnectionStrings__GradeGenie).");

        services.AddDbContext<GradeGenieDbContext>(options =>
        {
            // Choose provider by inspecting the connection string. If it looks like SQL Server, use SqlServer; otherwise use SQLite.
            if (connectionString.IndexOf("Server=", StringComparison.OrdinalIgnoreCase) >= 0 || connectionString.IndexOf("Initial Catalog=", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                options.UseSqlServer(connectionString);
            }
            else
            {
                options.UseSqlite(connectionString);
            }
        });
        services.Configure<AiProviderOptions>(options =>
        {
            options.Endpoint = configuration["AiProvider:Endpoint"] ?? string.Empty;
            options.ApiKey = configuration["AiProvider:ApiKey"] ?? string.Empty;
            options.Model = configuration["AiProvider:Model"] ?? string.Empty;
        });
        services.AddHttpClient<IAcademicInsightsProvider, HttpAcademicInsightsProvider>();
        services.AddScoped<IStudentRepository, StudentRepository>();
        return services;
    }
}
