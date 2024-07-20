
using System.Diagnostics;
using Content.Datum;
using Content.Datum.Data;
using Content.Datum.Data.FileApis;
using Content.Datum.Services;
using Content.Downloader.UI;
using ContentDownloader.Data;
using Microsoft.Extensions.DependencyInjection;
using Terminal.Gui;

namespace Content.Downloader;

public static class Program
{
    public static RobustUrl? Url;

    public static ContentApp ContentApp;
    
    public static void Main(string[] args)
    {

        ContentApp = new ContentApp();
        ContentApp.ServiceCollection.AddSingleton<ServerListWindow>();
        ContentApp.ServiceCollection.AddSingleton<ILogger, ConsoleLogger>();
        ContentApp.Build();
        Run();
    }
    
    public static void Run()
    {
        do
        {
            Url = null;
            var window = ContentApp.ServiceProvider.GetService<ServerListWindow>()!;
            Application.QuitKey = Key.C.WithCtrl;
            Task.Run(window.LoadData);
            Application.Init();
            Application.Run(window);
            Application.Shutdown();
        
            if(Url is null) continue;
            
            var task = Task.Run(Download);
            task.Wait();
        } while (Url is not null);
    }
    
    public static async Task Download()
    {
        using (var cancelTokenSource = new CancellationTokenSource())
        {
            var downloadService = ContentApp.ServiceProvider.GetService<ContentService>()!;
            var buildInfo = await downloadService.GetBuildInfo(Url!, cancelTokenSource.Token);
            var path = Path.GetTempPath() + buildInfo.BuildInfo.build.hash + "\\";
            
            await downloadService.Unpack(buildInfo.RobustManifestInfo, new FileApi(path), cancelTokenSource.Token);
            Process.Start(new ProcessStartInfo("explorer.exe", path));
        }
    }
}