using Content.Datum.Services;
using Content.Runner.Services;
using Content.Runner.UI;
using Microsoft.Extensions.DependencyInjection;

namespace Content.Runner;

public static class Dependencies
{
    public static void InitializeUI(IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<ServerListWindow>();
        serviceCollection.AddSingleton<AuthWindow>();
    }
    public static void Initialize(IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<AuthService>();
        serviceCollection.AddSingleton<ILogger, ConsoleLogger>();
    }
}