using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FocusFlow.Infrastructure.Persistence;

public static class DatabaseInitializer
{
    public static void InitializeFocusFlowDatabase(
        this IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(services);

        using IServiceScope scope =
            services.CreateScope();

        FocusFlowDbContext dbContext =
            scope.ServiceProvider
                .GetRequiredService<
                    FocusFlowDbContext>();

        dbContext.Database.Migrate();
    }
}