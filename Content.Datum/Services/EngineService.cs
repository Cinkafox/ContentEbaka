using System.Diagnostics.CodeAnalysis;
using Content.Datum.Data;
using Content.Datum.Data.FileApis;
using Content.Datum.Data.FileApis.Interfaces;
using Content.Datum.Utils;
using ContentDownloader.Utils;
using Robust.LoaderApi;

namespace Content.Datum.Services;

public class EngineService
{
    private readonly RestService _restService;
    private readonly DebugService _debugService;
    private readonly VarService _varService;
    private readonly FileService _fileService;
    private readonly IServiceProvider _serviceProvider;
    private readonly AssemblyService _assemblyService;

    public Dictionary<string, VersionInfo> VersionInfos;
    public Dictionary<string, Module> ModuleInfos;

    public EngineService(RestService restService, DebugService debugService, VarService varService,
        FileService fileService, IServiceProvider serviceProvider, AssemblyService assemblyService)
    {
        _restService = restService;
        _debugService = debugService;
        _varService = varService;
        _fileService = fileService;
        _serviceProvider = serviceProvider;
        _assemblyService = assemblyService;

        var loadTask = Task.Run(() => LoadEngineManifest(CancellationToken.None));
        loadTask.Wait();
    }

    public async Task LoadEngineManifest(CancellationToken cancellationToken)
    {
        var info = await _restService.GetAsync<Dictionary<string, VersionInfo>>(
            _varService.EngineManifestUrl,cancellationToken);
        var moduleInfo = await _restService.GetAsync<ModulesInfo>(
            _varService.EngineModuleManifestUrl, cancellationToken);
        
        if(info.Value is null) return;
        VersionInfos = info.Value;
        
        if(moduleInfo.Value is null) return;
        ModuleInfos = moduleInfo.Value.Modules;
        
        foreach (var f in ModuleInfos.Keys)
        {
            _debugService.Debug(f);
        }
    }

    public BuildInfo? GetVersionInfo(string version)
    {
        if(!VersionInfos.TryGetValue(version, out var foundVersion))
            return null;

        if (foundVersion.RedirectVersion != null) 
            return GetVersionInfo(foundVersion.RedirectVersion);

        var bestRid = RidUtility.FindBestRid(foundVersion.Platforms.Keys);
        if (bestRid == null)
        {
            bestRid = "linux-x64";
        }

        _debugService.Log("Selecting RID" + bestRid);
        
        return foundVersion.Platforms[bestRid];
    }

    public bool TryGetVersionInfo(string version,[NotNullWhen(true)] out BuildInfo? info)
    {
        info = GetVersionInfo(version);
        return info != null;
    }

    public async Task<AssemblyApi?> EnsureEngine(string version)
    {
        _debugService.Log("Ensure engine " + version);
        
        if (!TryOpen(version))
        {
            await DownloadEngine(version);
        }

        try
        {
            return _assemblyService.Mount(_fileService.OpenZip(version, _fileService.EngineFileApi));
        }
        catch (Exception e)
        {
            _fileService.EngineFileApi.Remove(version);
            throw;
        }
    }

    public async Task DownloadEngine(string version)
    {
        if(!TryGetVersionInfo(version,out var info))
            return;
        
        _debugService.Log("Downloading engine version " + version);
        using var client = new HttpClient();
        var s = await client.GetStreamAsync(info.Url);
        _fileService.EngineFileApi.Save(version, s);
        await s.DisposeAsync();
    }

    public bool TryOpen(string version,[NotNullWhen(true)] out Stream? stream)
    {
        return _fileService.EngineFileApi.TryOpen(version, out stream);
    }

    public bool TryOpen(string version)
    {
        var a = TryOpen(version, out var stream);
        if(a) stream!.Close();
        return a;
    }

    public BuildInfo? GetModuleBuildInfo(string moduleName, string version)
    {
        if (!ModuleInfos.TryGetValue(moduleName, out var module) || !module.Versions.TryGetValue(version, out var value))
            return null;
        
        var bestRid = RidUtility.FindBestRid(value.Platforms.Keys);
        if (bestRid == null)
        {
            throw new Exception("No engine version available for our platform!");
        }

        return value.Platforms[bestRid];
    }
    
    public bool TryGetModuleBuildInfo(string moduleName, string version,[NotNullWhen(true)] out BuildInfo? info)
    {
        info = GetModuleBuildInfo(moduleName, version);
        return info != null;
    }

    public string ResolveModuleVersion(string moduleName, string engineVersion)
    {
        var engineVersionObj = Version.Parse(engineVersion);
        var module = ModuleInfos[moduleName];
        var selectedVersion = module.Versions.Select(kv => new { Version = Version.Parse(kv.Key), kv.Key, kv.Value })
            .Where(kv => engineVersionObj >= kv.Version)
            .MaxBy(kv => kv.Version);

        if (selectedVersion == null) throw new Exception();
        
        return selectedVersion.Key;
    }

    public async Task<AssemblyApi?> EnsureEngineModules(string moduleName, string engineVersion)
    {
        var moduleVersion = ResolveModuleVersion(moduleName, engineVersion);
        if (!TryGetModuleBuildInfo(moduleName, moduleVersion, out var buildInfo))
            return null;

        var fileName = ConcatName(moduleName, moduleVersion);

        if (!TryOpen(fileName))
        {
            await DownloadEngineModule(moduleName, moduleVersion);
        }
        
        try
        {
            return _assemblyService.Mount(_fileService.OpenZip(fileName, _fileService.EngineFileApi));
        }
        catch (Exception e)
        {
            _fileService.EngineFileApi.Remove(fileName);
            throw;
        }
    }
    
    public async Task DownloadEngineModule(string moduleName,string moduleVersion)
    {
        if (!TryGetModuleBuildInfo(moduleName, moduleVersion, out var info))
            return;
        
        _debugService.Log("Downloading engine module version " + moduleVersion);
        using var client = new HttpClient();
        var s = await client.GetStreamAsync(info.Url);
        _fileService.EngineFileApi.Save(ConcatName(moduleName,moduleVersion), s);
        await s.DisposeAsync();
    }

    public string ConcatName(string moduleName, string moduleVersion)
    {
        return moduleName + "" + moduleVersion;
    }
}