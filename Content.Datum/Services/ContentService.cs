using System.Buffers.Binary;
using System.Data;
using System.Globalization;
using System.Net.Http.Headers;
using System.Numerics;
using Content.Datum.Data;
using Content.Datum.Data.FileApis;
using Content.Datum.Data.FileApis.Interfaces;
using Content.Datum.Utils;
using ContentDownloader.Utils;

namespace Content.Datum.Services;

public partial class ContentService
{
    private readonly RestService _restService;
    private readonly DebugService _debugService;
    private readonly VarService _varService;
    private readonly FileService _fileService;
    private readonly EngineService _engineService;
    private readonly AssemblyService _assemblyService;
    private readonly HttpClient _http = new HttpClient();

    public ContentService(RestService restService, DebugService debugService, VarService varService, 
        FileService fileService, EngineService engineService, AssemblyService assemblyService)
    {
        _restService = restService;
        _debugService = debugService;
        _varService = varService;
        _fileService = fileService;
        _engineService = engineService;
        _assemblyService = assemblyService;
    }

    public async Task<RobustBuildInfo> GetBuildInfo(RobustUrl url,CancellationToken cancellationToken)
    {
        var info = new RobustBuildInfo();
        info.Url = url;
        var bi = await _restService.GetAsync<Info>(url.InfoUri,cancellationToken);
        if (bi.Value is null) throw new NoNullAllowedException();
        info.BuildInfo = bi.Value;
        info.RobustManifestInfo = info.BuildInfo.build.acz ? 
            new RobustManifestInfo(new RobustPath(info.Url, "manifest.txt"), new RobustPath(info.Url, "download"),bi.Value.build.manifest_hash) : 
            new RobustManifestInfo(new Uri(info.BuildInfo.build.manifest_url),new Uri(info.BuildInfo.build.manifest_download_url),bi.Value.build.manifest_hash);

        return info;
    }
    
    
}