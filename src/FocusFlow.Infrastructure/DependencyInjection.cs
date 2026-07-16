using FocusFlow.Application.Abstractions.Persistence;
using FocusFlow.Application.Abstractions.Time;
using FocusFlow.Infrastructure.Persistence;
using FocusFlow.Infrastructure.Persistence.Repositories;
using FocusFlow.Infrastructure.Time;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FocusFlow.Infrastructure;

public static class InfrastructureDependencyInjection
{
    public static IServiceCollection AddFocusFlowInfrastructure(
        this IServiceCollection services,
        string databasePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            databasePath);

        string? databaseDirectory =
            Path.GetDirectoryName(databasePath);

        if (!string.IsNullOrWhiteSpace(
                databaseDirectory))
        {
            Directory.CreateDirectory(
                databaseDirectory);
        }

        var connectionStringBuilder =
            new SqliteConnectionStringBuilder
            {
                DataSource = databasePath,
                Mode = SqliteOpenMode.ReadWriteCreate,
                Cache = SqliteCacheMode.Shared,
                ForeignKeys = true
            };

        services.AddDbContext<FocusFlowDbContext>(
            options =>
            {
                options.UseSqlite(
                    connectionStringBuilder
                        .ToString());
            });

        services.AddScoped<
            IFocusTaskRepository,
            FocusTaskRepository>();

        services.AddScoped<
            IPomodoroPresetRepository,
            PomodoroPresetRepository>();

        services.AddScoped<
            IFocusSessionRepository,
            FocusSessionRepository>();

        services.AddScoped<IUnitOfWork>(
            serviceProvider =>
                serviceProvider.GetRequiredService<
                    FocusFlowDbContext>());

        services.AddSingleton<
            IAppTimeProvider,
            SystemAppTimeProvider>();

        return services;
    }
}