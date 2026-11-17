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
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Server=(localdb)\\mssqllocaldb;Database=TestPilotDb;Trusted_Connection=True;MultipleActiveResultSets=true";

        services.AddDbContext<TestPilotDbContext>(options =>
            options.UseSqlServer(connectionString, b => b.MigrationsAssembly(typeof(TestPilotDbContext).Assembly.FullName)));

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<TestPilotDbContext>());

        // Security & Token generation
        services.AddSingleton<IPasswordHasherService, PasswordHasherService>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        // HTTP Client Factory
        services.AddHttpClient("TestPilotClient", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        return services;
    }
}
