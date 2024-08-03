using System.Diagnostics.CodeAnalysis;
using Content.Datum.Data;
using Content.Datum.Data.FileApis;
using Content.Datum.Data.FileApis.Interfaces;
using Content.Datum.Utils;
using ContentDownloader.Data;
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

    public Dictionary<string, VersionInfo> VersionInfos;

    public EngineService(RestService restService, DebugService debugService, VarService varService,FileService fileService, IServiceProvider serviceProvider)
    {
        _restService = restService;
        _debugService = debugService;
        _varService = varService;
        _fileService = fileService;
        _serviceProvider = serviceProvider;

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

        var bestRid = RidUtility.FindBestRid(foundVersion.Platforms.Keys);
        if (bestRid == null)
        {
            bestRid = "linux-x64";
            //throw new Exception("No engine version available for our platform!");
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
        if(!TryGetVersionInfo(version,out var info))
            return null;
        
        if (!TryGetEngine(version))
        {
            await DownloadEngine(version);
        }

        try
        {
            return new AssemblyApi(_fileService.OpenZip(version, _fileService.EngineFileApi),_serviceProvider);
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

    public bool TryGetEngine(string version,[NotNullWhen(true)] out Stream? stream)
    {
        return _fileService.EngineFileApi.TryOpen(version, out stream);
    }

    public bool TryGetEngine(string version)
    {
        var a = TryGetEngine(version, out var stream);
        if(a) stream!.Close();
        return a;
    }
}