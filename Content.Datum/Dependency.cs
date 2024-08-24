using Content.Datum.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Content.Datum;

public static class Dependency
{
    public static void Initialize(IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<ContentService>();
        serviceCollection.AddSingleton<DebugService>();
        serviceCollection.AddSingleton<EngineService>();
        serviceCollection.AddSingleton<FileService>();
        serviceCollection.AddSingleton<RestService>();
        serviceCollection.AddSingleton<VarService>();
        serviceCollection.AddSingleton<AssemblyService>();
        serviceCollection.AddSingleton<ReflectionService>();
        serviceCollection.AddSingleton<DynamicService>();
    }
}