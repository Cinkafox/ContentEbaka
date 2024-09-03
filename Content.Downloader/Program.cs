using Content.Datum;

namespace Content.Downloader;

public static class Program
{
    public static void Main(string[] args)
    {
        new ContentApp()
            .RegisterDependencies<DownloadDependencies>()
            .RegisterDependencies<UIDependencies>()
            .Build()
            .Run(args);
    }
}