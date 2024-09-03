using Content.Datum.Data;
using Content.Datum.Services;
using Content.Downloader.Services;
using Content.UI.Console.UI;
using Microsoft.Extensions.DependencyInjection;

namespace Content.Downloader;

public static class Dependencies
{
    public static void InitializeUI(IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<ServerListWindow>();
    }

    public static void Initialize(IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<ILogger, ConsoleLogger>();
        serviceCollection.AddSingleton<IExecutePoint, ApplicationService>();
    }
}