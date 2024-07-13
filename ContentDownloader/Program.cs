using System.Diagnostics;
using ContentDownloader.Data;
using ContentDownloader.Services;
using ContentDownloader.Ui;
using ContentDownloader.Utils;

using Terminal.Gui;

namespace ContentDownloader;

public static class Programm
{
    public static RobustUrl? Url;
    public static void Main(string[] args)
    {
        Run();
    }

    public static void Run()
    {
        do
        {
            Url = null;
            var window = new ServerListWindow();
            Application.QuitKey = Key.C.WithCtrl;
            Task.Run(window.LoadData);
            Application.Run(window);
            Application.Shutdown();
        
            if(Url is null) continue;
            
            ConstServices.Logger.Log("DOWNLOAD " + Url);
            var task = Task.Run(Download);
            task.Wait();
        } while (Url is not null);
    }
    
    public static async Task Download()
    {
        using (var cancelTokenSource = new CancellationTokenSource())
        {
            var rbi = new RobustBuildInfo(Url!);
            var contentHolder = new ContentHolder(rbi);
            await contentHolder.EnsureItems(cancelTokenSource.Token);
            var path = Path.GetTempPath() + rbi.Url.Uri.Host + "\\";
            await contentHolder.ContentDownloader.Unpack(path, cancelTokenSource.Token);
            Process.Start(new ProcessStartInfo("explorer.exe", path));
        }
    }
}