using System.Reflection;
using Content.Datum.Services;

namespace Content.Script.Services;

public class BaseAssemblyProviderService(AssemblyService assemblyService, FileService fileService)
{
    private Assembly _robustClient;
    private Assembly _robustShared;
    private Assembly _contentClient;
    private Assembly _contentShared;

    public Assembly RobustClient => _robustClient;
    public Assembly RobustShared => _robustShared;
    public Assembly ContentClient => _contentClient;
    public Assembly ContentShared => _contentShared;

    public void LoadEngineAssemblies()
    {
        if (!assemblyService.TryGetCachedAssembly("Robust.Client", out _robustClient) ||
            !assemblyService.TryGetCachedAssembly("Robust.Shared", out _robustShared))
            throw new Exception();
    }
    
    public void LoadContentAssemblies()
    {
        var asm = assemblyService.Mount(fileService.HashApi);

        if (!assemblyService.TryOpenAssembly("Assemblies/Content.Client", asm, out var _contentClient) ||
            !assemblyService.TryOpenAssembly("Assemblies/Content.Shared", asm, out var _contentShared)) 
            throw new Exception();
    }
}