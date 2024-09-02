using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Content.Datum.Data;
using Content.Datum.Data.FileApis;
using Content.Datum.Services;
using Content.UI.Console.UI;
using Terminal.Gui;

namespace Content.Downloader.Services;

public class ApplicationService(
    DebugService debugService,
    ContentService contentService,
    ServerListWindow serverListWindow)
    : IExecutePoint
{
    public RobustUrl? Url { get; private set; }

    public void Run(string[] args)
    {
        if (!TryGetUrl(args, out var selectedUrl))
        {
            var window = serverListWindow;
            Application.QuitKey = Key.C.WithCtrl;
            Task.Run(window.LoadData);
            Application.Init();
            Application.Run(window);
            Application.Shutdown();

            selectedUrl = window.SelectedUrl;
        }
        
        try
        {
            if (selectedUrl is null) 
                throw new Exception("Exit with no url");
            Url = new RobustUrl(selectedUrl);
        }
        catch (Exception e)
        {
            debugService.Log($"ERROR WHILE PARSING {selectedUrl}: {e.Message}");
            return;
        }
            
        var task = Task.Run(Download);
        task.Wait();
    }

    private bool TryGetUrl(string[] args, [NotNullWhen(true)] out string? url)
    {
        url = null;
        if (args.Length == 0) return false;
        
        url = args[0];
        return true;
    }
    
    private async Task Download()
    {
        using var cancelTokenSource = new CancellationTokenSource();
        var buildInfo = await contentService.GetBuildInfo(Url!, cancelTokenSource.Token);
        var path = Path.GetTempPath() + buildInfo.BuildInfo.build.manifest_hash + "\\";
        
        if(!Directory.Exists(path))
            await contentService.Unpack(buildInfo.RobustManifestInfo, new FileApi(path), cancelTokenSource.Token);
        
        debugService.Log("Opening file: " + path);
        Process.Start(new ProcessStartInfo("explorer.exe", path));
    }
}