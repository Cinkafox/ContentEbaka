using System.Reflection;
using ContentDownloader.Data;
using HarmonyLib;
using Microsoft.Extensions.DependencyInjection;
using Robust.LoaderApi;

namespace Content.Datum.Services;

public partial class ContentService
{
    public event Action<ContentRunEvent>? OnContentRun;
    
    public async Task Run(string[] runArgs,RobustBuildInfo buildInfo,CancellationToken cancellationToken)
    {
        _debugService.Log("Start Content!");

        var engine = await _engineService.EnsureEngine(buildInfo.BuildInfo.build.engine_version);

        if (engine is null)
        {
            throw new Exception("Engine version is not usable: " + buildInfo.BuildInfo.build.engine_version);
        }

        await EnsureItems(buildInfo.RobustManifestInfo, cancellationToken);

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
        
        OnContentRun?.Invoke(new ContentRunEvent(engine,_fileService.HashApi));
        
        await Task.Run(() => loader.Main(args), cancellationToken);
    }
}

public class FuckRedialApi : IRedialApi
{
    public void Redial(Uri uri, string text = "")
    {
        
    }
}

public record struct ContentRunEvent(IFileApi Engine, IFileApi Content);

