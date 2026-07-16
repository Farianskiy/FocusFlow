using FocusFlow.Application.Features.Sessions.StartFocusSession;
using Microsoft.Extensions.DependencyInjection;

namespace FocusFlow.Application;

public static class ApplicationDependencyInjection
{
    public static IServiceCollection AddFocusFlowApplication(
        this IServiceCollection services)
    {
        services.AddScoped<StartFocusSessionHandler>();

        return services;
    }
}