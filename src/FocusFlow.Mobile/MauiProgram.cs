using FocusFlow.Application;
using FocusFlow.Infrastructure;
using FocusFlow.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;

namespace FocusFlow.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder =
            MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(
                fonts =>
                {
                    fonts.AddFont(
                        "OpenSans-Regular.ttf",
                        "OpenSansRegular");

                    fonts.AddFont(
                        "OpenSans-Semibold.ttf",
                        "OpenSansSemibold");
                });

        string databasePath =
            Path.Combine(
                Microsoft.Maui.Storage
                    .FileSystem
                    .Current
                    .AppDataDirectory,
                "focusflow.db3");

        builder.Services
            .AddFocusFlowApplication();

        builder.Services
            .AddFocusFlowInfrastructure(
                databasePath);

#if DEBUG
        builder.Logging.AddDebug();
#endif

        MauiApp app =
            builder.Build();

        app.Services
            .InitializeFocusFlowDatabase();

        return app;
    }
}