using Microsoft.Extensions.DependencyInjection;
using TestPilot.Application.Interfaces;
using TestPilot.Application.Services;

namespace TestPilot.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICollectionService, CollectionService>();
        services.AddScoped<IApiRequestService, ApiRequestService>();
        services.AddSingleton<IAssertionEvaluator, AssertionEvaluator>();
        services.AddScoped<ITestRunnerService, TestRunnerService>();
        return services;
    }
}
