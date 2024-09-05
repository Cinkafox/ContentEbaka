using Content.Datum.Data;
using Content.Datum.Services;
using Content.Script.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Content.Script;

public sealed class ScriptDependencies : IDependencyCollection
{
    public void Register(IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<BaseAssemblyProviderService>();
        serviceCollection.AddSingleton<HarmonyService>();
        serviceCollection.AddSingleton<ReflectionService>();
        serviceCollection.AddSingleton<PatchService>();
    }
}