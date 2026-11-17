using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TestPilot.Application.Interfaces;
using TestPilot.Infrastructure.Data;
using TestPilot.Infrastructure.Services;

namespace TestPilot.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var useInMemory = configuration.GetValue<bool>("UseInMemoryDatabase");

        if (useInMemory)
        {
            services.AddDbContext<TestPilotDbContext>(options =>
                options.UseInMemoryDatabase("TestPilotDevDb"));
        }
        else
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? "Server=(localdb)\\mssqllocaldb;Database=TestPilotDb;Trusted_Connection=True;MultipleActiveResultSets=true";

            services.AddDbContext<TestPilotDbContext>(options =>
                options.UseSqlServer(connectionString, b => b.MigrationsAssembly(typeof(TestPilotDbContext).Assembly.FullName)));
        }

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<TestPilotDbContext>());

        // Security, Token Generation & SSRF
        services.AddSingleton<IPasswordHasherService, PasswordHasherService>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddSingleton<ISsrfProtectionService, SsrfProtectionService>();

        // HTTP Client & Execution Engine
        services.AddHttpClient("TestPilotClient", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddScoped<IRequestExecutionService, RequestExecutionService>();

        // AI Test Suggestion Service
        services.AddScoped<IAiSuggestionService, AiSuggestionService>();

        return services;
    }
}
