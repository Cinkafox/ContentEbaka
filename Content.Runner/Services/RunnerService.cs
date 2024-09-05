using Content.Datum.Data;
using Content.Datum.Services;
using Content.Script.Services;
using Robust.LoaderApi;

namespace Content.Runner.Services;

public class RunnerService
{
    private readonly AssemblyService _assemblyService;
    private readonly BaseAssemblyProviderService _baseAssemblyProviderService;
    private readonly HarmonyService _harmonyService;
    private readonly PatchService _patchService;
    private readonly ContentService _contentService;
    private readonly DebugService _debugService;
    private readonly EngineService _engineService;
    private readonly FileService _fileService;
    private readonly VarService _varService;

    public RunnerService(ContentService contentService, DebugService debugService, VarService varService,
        FileService fileService, EngineService engineService, AssemblyService assemblyService, 
        BaseAssemblyProviderService baseAssemblyProviderService, HarmonyService harmonyService, PatchService patchService)
    {
        _contentService = contentService;
        _debugService = debugService;
        _varService = varService;
        _fileService = fileService;
        _engineService = engineService;
        _assemblyService = assemblyService;
        _baseAssemblyProviderService = baseAssemblyProviderService;
        _harmonyService = harmonyService;
        _patchService = patchService;
    }

    public async Task Run(string[] runArgs, RobustBuildInfo buildInfo, IRedialApi redialApi,
        CancellationToken cancellationToken)
    {
        _debugService.Log("Start Content!");

        var engine = await _engineService.EnsureEngine(buildInfo.BuildInfo.build.engine_version);

        if (engine is null)
            throw new Exception("Engine version is not usable: " + buildInfo.BuildInfo.build.engine_version);
        
        if (!_assemblyService.TryOpenAssembly(_varService.RobustAssemblyName, engine, out var clientAssembly))
            throw new Exception("Unable to locate Robust.Client.dll in engine build!");

        if (!_assemblyService.TryGetLoader(clientAssembly, out var loader))
            return;
        
        _baseAssemblyProviderService.LoadEngineAssemblies();
        
        _harmonyService.CreateInstance();
        
        Environment.SetEnvironmentVariable("HARMONY_DEBUG", "1");
        
        await _contentService.EnsureItems(buildInfo.RobustManifestInfo, cancellationToken);
        _baseAssemblyProviderService.LoadContentAssemblies();
        
        new Thread(() => _patchService.Boot()).Start();

        var extraMounts = new List<ApiMount>
        {
            new(_fileService.HashApi, "/")
        };

        var args = new MainArgs(runArgs, engine, redialApi, extraMounts);

        loader.Main(args);
    }
}