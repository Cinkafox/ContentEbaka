namespace ContentDownloader;

public static class Programm
{
    public static async Task Main(string[] args)
    {
        var a = new RobustBuildInfo((RobustUrl)"ss14s://game1.station14.ru/wl/server/");
        var b = new Downloader(a);
        await b.Unpack("./" + b.Info.BuildInfo.build.hash + "/");
        
    }
}
