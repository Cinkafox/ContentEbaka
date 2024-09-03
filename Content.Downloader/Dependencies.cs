using Content.Datum.Data;
using Content.Datum.Services;
using Content.Downloader.Services;
using Content.UI.Console.UI;
using Microsoft.Extensions.DependencyInjection;

namespace Content.Downloader;

public sealed class UIDependencies : IDependencyCollection
{
    public void Register(IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<ServerListWindow>();
    }
}

public sealed class DownloadDependencies : IDependencyCollection
{
    public void Register(IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<ILogger, ConsoleLogger>();
        serviceCollection.AddSingleton<IExecutePoint, ApplicationService>();
    }
}