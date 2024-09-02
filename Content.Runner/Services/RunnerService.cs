using Content.Datum.Data;
using Content.Datum.Services;
using Content.Runner.Data;
using Robust.LoaderApi;

namespace Content.Runner.Services;

public class RunnerService
{
    private readonly ContentService _contentService;
    private readonly DebugService _debugService;
    private readonly VarService _varService;
    private readonly FileService _fileService;
    private readonly EngineService _engineService;
    private readonly AssemblyService _assemblyService;
    private readonly AuthService _authService;

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
    
    public async Task Run(string[] runArgs,RobustBuildInfo buildInfo,CancellationToken cancellationToken)
    {
        _debugService.Log("Start Content!");

        var engine = await _engineService.EnsureEngine(buildInfo.BuildInfo.build.engine_version);

        if (engine is null)
        {
            throw new Exception("Engine version is not usable: " + buildInfo.BuildInfo.build.engine_version);
        }

        await _contentService.EnsureItems(buildInfo.RobustManifestInfo, cancellationToken);

        var extraMounts = new List<ApiMount>()
        {
            new ApiMount(_fileService.HashApi, "/")
        };

        var module = await _engineService.EnsureEngineModules("Robust.Client.WebView", buildInfo.BuildInfo.build.engine_version);
        if(module is not null)
            extraMounts.Add( new ApiMount(module, "/"));
        
        var args = new MainArgs(runArgs, engine, new FuckRedialApi(), extraMounts);
        
        if (!_assemblyService.TryOpenAssembly(_varService.RobustAssemblyName, engine, out var clientAssembly))
        {
            throw new Exception("Unable to locate Robust.Client.dll in engine build!");
        }

        if (!_assemblyService.TryGetLoader(clientAssembly, out var loader))
            return;
        
        await Task.Run(() => loader.Main(args), cancellationToken);
    }
    
    public async Task RunGame(RobustUrl url)
    {
        using var cancelTokenSource = new CancellationTokenSource();
        var buildInfo = await _contentService.GetBuildInfo(url, cancelTokenSource.Token);

        if (buildInfo.BuildInfo.auth.mode != "Disabled" && _authService.CurrentLogin != null)
        {
            var account = _authService.CurrentLogin;
            Environment.SetEnvironmentVariable("ROBUST_AUTH_TOKEN", account.Token.Token);
            Environment.SetEnvironmentVariable("ROBUST_AUTH_USERID", account.UserId.ToString());
            Environment.SetEnvironmentVariable("ROBUST_AUTH_PUBKEY", buildInfo.BuildInfo.auth.public_key);
            Environment.SetEnvironmentVariable("ROBUST_AUTH_SERVER", "https://auth.spacestation14.com/");
        }
            
        var args = new List<string>
        {
            // Pass username to launched client.
            // We don't load username from client_config.toml when launched via launcher.
            "--username", _authService.CurrentLogin?.Username ?? "Alise",

            // Tell game we are launcher
            "--cvar", "launch.launcher=true"
        };

        var connectionString = url.ToString();
        if (!string.IsNullOrEmpty(buildInfo.BuildInfo.connect_address))
            connectionString = buildInfo.BuildInfo.connect_address;
                
        // We are using the launcher. Don't show main menu etc..
        // Note: --launcher also implied --connect.
        // For this reason, content bundles do not set --launcher.
        args.Add("--launcher");

        args.Add("--connect-address");
        args.Add(connectionString);
                
        args.Add("--ss14-address");
        args.Add(url.ToString());

        await Run(args.ToArray(),buildInfo, cancelTokenSource.Token);
    }
}