using Content.Datum;
using Content.Datum.Services;
using Content.Runner.Services;
using Content.Runner.UI;
using ContentDownloader.Data;
using Microsoft.Extensions.DependencyInjection;
using Terminal.Gui;

namespace Content.Runner;

public static class Program
{
    public static RobustUrl? Url;

    public static ContentApp ContentApp;
    
    public static void Main(string[] args)
    {

        ContentApp = new ContentApp();
        Dependencies.Initialize(ContentApp.ServiceCollection);
        Dependencies.InitializeUI(ContentApp.ServiceCollection);
        ContentApp.Build();
        Task.Run(Run).Wait();
    }
    
    public static async Task Run()
    {
        do
        {
            Url = null;
            
            var window = ContentApp.ServiceProvider.GetService<AuthWindow>()!;
            var w = ContentApp.ServiceProvider.GetService<ServerListWindow>()!;

            Application.QuitKey = Key.C.WithCtrl;
            await w.LoadData();
            var debug = ContentApp.ServiceProvider.GetService<DebugService>()!;
            debug.Debug("Ensuring auth data...");
            if (await ContentApp.ServiceProvider.GetService<AuthService>()!.EnsureToken())
            {
                debug.Debug("Success!");
                Application.Init();
                Application.Run(w);
            }
            else
            {
                debug.Debug("Auth is required");
                Application.Init();
                Application.Run(window);
            }
            
            
            Application.Shutdown();
        
            if(Url is null) continue;
            
            var task = Task.Run(RunGame);
            task.Wait();
        } while (Url is not null && false);
    }

    public static async Task RunGame()
    {
        using (var cancelTokenSource = new CancellationTokenSource())
        {
            var contentService = ContentApp.ServiceProvider.GetService<ContentService>()!;
            var authService = ContentApp.ServiceProvider.GetService<AuthService>()!;
            var buildInfo = await contentService.GetBuildInfo(Url!, cancelTokenSource.Token);

            if (buildInfo.BuildInfo.auth.mode != "Disabled" && authService.CurrentLogin != null)
            {
                var account = authService.CurrentLogin;
                Environment.SetEnvironmentVariable("ROBUST_AUTH_TOKEN", account.Token.Token);
                Environment.SetEnvironmentVariable("ROBUST_AUTH_USERID", account.UserId.ToString());
                Environment.SetEnvironmentVariable("ROBUST_AUTH_PUBKEY", buildInfo.BuildInfo.auth.public_key);
                Environment.SetEnvironmentVariable("ROBUST_AUTH_SERVER", "https://auth.spacestation14.com/");
            }
            
            var args = new List<string>
            {
                // Pass username to launched client.
                // We don't load username from client_config.toml when launched via launcher.
                "--username", authService.CurrentLogin?.Username ?? "Alise",

                // Tell game we are launcher
                "--cvar", "launch.launcher=true"
            };
            
            if (Url != null)
            {
                var connectionString = Url.ToString();
                if (!string.IsNullOrEmpty(buildInfo.BuildInfo.connect_address))
                    connectionString = buildInfo.BuildInfo.connect_address;
                
                // We are using the launcher. Don't show main menu etc..
                // Note: --launcher also implied --connect.
                // For this reason, content bundles do not set --launcher.
                args.Add("--launcher");

                args.Add("--connect-address");
                args.Add(connectionString);
                
                args.Add("--ss14-address");
                args.Add(Url.ToString());
                
                Console.WriteLine("CONNECTING " + Url + " >" + connectionString );
            }
            
            await contentService.Run(args.ToArray(),buildInfo, cancelTokenSource.Token);
        }
    }
}