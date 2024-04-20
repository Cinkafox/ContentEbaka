using Robust.LoaderApi;

namespace ContentDownloader;

public static class Programm
{
    public static async Task Main(string[] args)
    {
        ConstServices.Logger.Log("Starting some shit");
        var a = new RobustBuildInfo((RobustUrl)"ss14://server.fishstation.ru");
        var cr = new ContentRunner(a);
        await cr.Run();
    }
}
