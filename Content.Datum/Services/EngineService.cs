using System.Diagnostics.CodeAnalysis;
using Content.Datum.Data;
using Content.Datum.Data.FileApis;
using Content.Datum.Data.FileApis.Interfaces;
using Content.Datum.Utils;
using ContentDownloader.Data;
using Robust.LoaderApi;

namespace Content.Datum.Services;

public class EngineService
{
    private readonly RestService _restService;
    private readonly DebugService _debugService;
    private readonly VarService _varService;
    private readonly FileService _fileService;
    
    public readonly IReadWriteFileApi EngineFileApi;

    public Dictionary<string, VersionInfo> VersionInfos;

    public EngineService(RestService restService, DebugService debugService, VarService varService,FileService fileService)
    {
        _restService = restService;
        _debugService = debugService;
        _varService = varService;
        _fileService = fileService;
        
        EngineFileApi = fileService.CreateAndMount("engine/");

        var loadTask = Task.Run(() => LoadEngineManifest(CancellationToken.None));
        loadTask.Wait();
    }

    public async Task LoadEngineManifest(CancellationToken cancellationToken)
    {
        var info = await _restService.GetAsync<Dictionary<string, VersionInfo>>(
            _varService.EngineManifestUrl,cancellationToken);
        if(info.Value is null) return;
        VersionInfos = info.Value;
    }

    public BuildInfo? GetVersionInfo(string version)
    {
        if(!VersionInfos.TryGetValue(version, out var foundVersion))
            return null;

        if (foundVersion.RedirectVersion != null) 
            return GetVersionInfo(foundVersion.RedirectVersion);

        var bestRid = "win-x64";//RidUtility.FindBestRid(foundVersion.Platforms.Keys);
        if (bestRid == null)
        {
            throw new Exception("No engine version available for our platform!");
        }

        _debugService.Log("Selecting RID" + bestRid);
        
        return foundVersion.Platforms[bestRid];
    }

    public bool TryGetVersionInfo(string version,[NotNullWhen(true)] out BuildInfo? info)
    {
        info = GetVersionInfo(version);
        return info != null;
    }

    public async Task<IFileApi> EnsureEngine(string version)
    {
        _debugService.Log("Ensure engine " + version);
        if(!TryGetVersionInfo(version,out var info))
            return null;
        
        if (!TryGetEngine(version))
        {
            await DownloadEngine(version);
        }
        
        return _fileService.OpenZip(_varService.EnginePath + version);
    }

    public async Task DownloadEngine(string version)
    {
        if(!TryGetVersionInfo(version,out var info))
            return;
        
        _debugService.Log("Downloading engine version " + version);
        using var client = new HttpClient();
        await using var s = await client.GetStreamAsync(info.Url);
        EngineFileApi.Save(version, s);
    }

    public bool TryGetEngine(string version,[NotNullWhen(true)] out Stream? stream)
    {
        return EngineFileApi.TryOpen(version, out stream);
    }

    public bool TryGetEngine(string version)
    {
        var a = TryGetEngine(version, out var stream);
        if(a) stream!.Close();
        return a;
    }
}