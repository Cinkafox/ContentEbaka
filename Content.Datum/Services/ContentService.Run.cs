using Content.Datum.Utils;
using ContentDownloader.Data;
using Robust.LoaderApi;

namespace Content.Datum.Services;

public partial class ContentService
{
    public async Task Run(RobustBuildInfo buildInfo,IEnumerable<ApiMount> extraMounts,CancellationToken cancellationToken)
    {
        _debugService.Log("Start Content!");
        if (!_engineService.TryGetEngine(buildInfo.BuildInfo.build.engine_version))
        {
            await _engineService.EnsureEngine(buildInfo.BuildInfo.build.engine_version);
            if (!_engineService.TryGetEngine(buildInfo.BuildInfo.build.engine_version)) 
                throw new Exception("Engine is not ensured");
        }

        var items = await EnsureItems(buildInfo.RobustManifestInfo, cancellationToken);
        var hashFileApi = _fileService.GetHashApi(items,ContentFileApi);

        var args = new MainArgs([], hashFileApi, new FuckRedialApi(), extraMounts);
        
        if (!_assemblyService.TryOpenAssembly(_varService.RobustAssemblyName, out var clientAssembly))
        {
            _debugService.Log("Unable to locate Robust.Client.dll in engine build!");
            return;
        }

        if (!_assemblyService.TryGetLoader(clientAssembly, out var loader))
            return;
        
        await Task.Run(() => loader.Main(args), cancellationToken);
    }
}

public class FuckRedialApi : IRedialApi
{
    public void Redial(Uri uri, string text = "")
    {
        
    }
}