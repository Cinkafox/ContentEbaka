using Content.Datum.Data;
using Content.Datum.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Content.Datum;

public sealed class BaseDependencies : IDependencyCollection
{
    public void Register(IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<ContentService>();
        serviceCollection.AddSingleton<DebugService>();
        serviceCollection.AddSingleton<EngineService>();
        serviceCollection.AddSingleton<FileService>();
        serviceCollection.AddSingleton<RestService>();
        serviceCollection.AddSingleton<VarService>();
        serviceCollection.AddSingleton<AssemblyService>();
    }
}