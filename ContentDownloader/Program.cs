using Robust.LoaderApi;

namespace ContentDownloader;

public static class Programm
{
    public static async Task Main(string[] args)
    {
        var a = new RobustBuildInfo((RobustUrl)"ss14s://game1.station14.ru/wl/server/");
        var b = new Downloader(a);
        await b.Unpack("./" + b.Info.BuildInfo.build.hash + "/");
        var engine = await ConstServices.EngineShit.EnsureEngine(a.BuildInfo.build.engine_version);
        
        FileApi contentApi = new FileApi("./" + b.Info.BuildInfo.build.hash + "/");
        IEnumerable<ApiMount> extraMounts = new[] { new ApiMount(contentApi, "/") };
        
        var aargs = new MainArgs([], engine.FileApi, new FuckRedialApi(), extraMounts);
        
        if (!engine.TryOpenAssembly(AssemblyHelper.RobustAssemblyName, out var clientAssembly))
        {
            Console.WriteLine("Unable to locate Robust.Client.dll in engine build!");
            return;
        }

        if (!AssemblyHelper.TryGetLoader(clientAssembly, out var loader))
            return;
        
        loader.Main(aargs);

    }
}

public class FuckRedialApi : IRedialApi
{
    public void Redial(Uri uri, string text = "")
    {
        
    }
}

public class FileApi : IFileApi
{
    public string RootPath;
    public FileApi(string rootPath)
    {
        RootPath = rootPath;
    }
    
    public bool TryOpen(string path, out Stream? stream)
    {
        if (File.Exists(RootPath + path))
        {
            stream = File.Open(RootPath + path,FileMode.Open);
            return true;
        }

        stream = null;
        return false;
    }

    public IEnumerable<string> AllFiles
    {
        get
        {
            return Directory.EnumerateFiles(RootPath, "*.*", SearchOption.AllDirectories);
        }
    }
}