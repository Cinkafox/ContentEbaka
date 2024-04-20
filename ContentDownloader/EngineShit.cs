using System.Diagnostics.CodeAnalysis;

namespace ContentDownloader;

public class EngineShit
{
    public static readonly Uri EngineManifestUrl = new Uri("https://central.spacestation14.io/builds/robust/manifest.json");
    public string EnginePath = "./engine/";
    public Dictionary<string, VersionInfo> VersionInfos;

    public EngineShit()
    {
        VersionInfos = ConstServices.RestService.GetDataSync<Dictionary<string, VersionInfo>>(EngineManifestUrl);
        if (!Directory.Exists(EnginePath)) Directory.CreateDirectory(EnginePath);
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

        Console.WriteLine("Selecting RID {0}", bestRid);
        
        return foundVersion.Platforms[bestRid];
    }

    public bool TryGetVersionInfo(string version,[NotNullWhen(true)] out BuildInfo? info)
    {
        info = GetVersionInfo(version);
        return info != null;
    }

    public async Task<AssemblyHelper?>  EnsureEngine(string version)
    {
        if(!TryGetVersionInfo(version,out var info))
            return null;
        
        Console.WriteLine("Ensure shit");
        if (!TryGetEngine(version))
        {
            Console.WriteLine("Download some shit");
            await DownloadEngine(version);
        }

        return new AssemblyHelper(ZipFileApi.FromPath(EnginePath + version));
    }

    public async Task DownloadEngine(string version)
    {
        if(!TryGetVersionInfo(version,out var info))
            return;
        
        Console.WriteLine("MEOW");
        using var client = new HttpClient();
        await using var s = await client.GetStreamAsync(info.Url);
        await using var fs = new FileStream(EnginePath + version, FileMode.OpenOrCreate);
        await s.CopyToAsync(fs);
    }

    public bool TryGetEngine(string version)
    {
        return File.Exists(EnginePath + version);
    }
}