using Content.Datum.Data;
using Content.Datum.Services;
using Robust.LoaderApi;

namespace Content.Runner.Services;

public class RunnerService
{
    private readonly AssemblyService _assemblyService;
    private readonly AuthService _authService;
    private readonly ContentService _contentService;
    private readonly DebugService _debugService;
    private readonly EngineService _engineService;
    private readonly FileService _fileService;
    private readonly VarService _varService;

    public RunnerService(ContentService contentService, DebugService debugService, VarService varService,
        FileService fileService, EngineService engineService, AssemblyService assemblyService, AuthService authService)
    {
        _contentService = contentService;
        _debugService = debugService;
        _varService = varService;
        _fileService = fileService;
        _engineService = engineService;
        _assemblyService = assemblyService;
        _authService = authService;
    }

    public async Task Run(string[] runArgs, RobustBuildInfo buildInfo, IRedialApi redialApi,
        CancellationToken cancellationToken)
    {
        _debugService.Log("Start Content!");

        var engine = await _engineService.EnsureEngine(buildInfo.BuildInfo.build.engine_version);

        if (engine is null)
            throw new Exception("Engine version is not usable: " + buildInfo.BuildInfo.build.engine_version);

        await _contentService.EnsureItems(buildInfo.RobustManifestInfo, cancellationToken);

        var extraMounts = new List<ApiMount>
        {
            new(_fileService.HashApi, "/")
        };

        var module =
            await _engineService.EnsureEngineModules("Robust.Client.WebView", buildInfo.BuildInfo.build.engine_version);
        if (module is not null)
            extraMounts.Add(new ApiMount(module, "/"));

        var args = new MainArgs(runArgs, engine, redialApi, extraMounts);

        if (!_assemblyService.TryOpenAssembly(_varService.RobustAssemblyName, engine, out var clientAssembly))
            throw new Exception("Unable to locate Robust.Client.dll in engine build!");

        if (!_assemblyService.TryGetLoader(clientAssembly, out var loader))
            return;

        await Task.Run(() => loader.Main(args), cancellationToken);
    }
}