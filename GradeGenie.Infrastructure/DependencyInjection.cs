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
        services.AddDbContext<GradeGenieDbContext>(options => options.UseSqlite(configuration.GetConnectionString("GradeGenie")));
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
