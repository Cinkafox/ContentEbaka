using Content.Datum.Services;
using HarmonyLib;

namespace Content.Script.Services;

public class ReflectionService(AssemblyService assemblyService, FileService fileService, DebugService debugService)
{
    public IEnumerable<string> TypePrefixes => _typePrefixes;

    // Cache these so that we only need to allocate the array ONCE.
    private static readonly string[] _typePrefixes = new[]
    {
        "",
        "Robust.Client.",
        "Robust.Shared.",
        "Content.Shared.",
        "Content.Client."
    };

    public Type? GetTypeImp(string name)
    {
        foreach (string prefix in TypePrefixes)
        {
            string appendedName = prefix + name;
            foreach (var assembly in assemblyService.Assemblies)
            {
                var theType = assembly.Value.GetType(appendedName);
                if (theType != null)
                {
                    return theType;
                }
            }
        }

        return null;
    }

    public Type? GetType(string name)
    {
        var prefix = ExtrackPrefix(name);
        return !assemblyService.TryGetCachedAssembly(prefix, out var assembly) 
            ? GetTypeImp(name)
            : assembly.GetType(name);
    }

    private string ExtrackPrefix(string path)
    {
        var sp = path.Split(".");
        return sp[0] + "." + sp[1];
    }
}