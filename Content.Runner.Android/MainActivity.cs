using System.Reflection;
using _Microsoft.Android.Resource.Designer;
using Android.OS;
using Android.Views;
using Content.Datum;
using Content.Datum.Data;
using Content.Datum.Services;
using Content.Runner.Android.Services;
using ContentDownloader.Data;
using HarmonyLib;
using Microsoft.Extensions.DependencyInjection;
using MonoMod;
using MonoMod.RuntimeDetour;
using Exception = System.Exception;

namespace Content.Runner.Android;

[Activity(Label = "@string/app_name", MainLauncher = true)]
public class MainActivity : Activity
{
    public ContentApp MainApp = new();

    private RestService _restService;
    
    Handler mainHandler = new Handler(Looper.MainLooper);
    
    public List<Uri> HubUris = new List<Uri>()
    { 
        new ("https://cdn.spacestationmultiverse.com/hub/api/servers"),
        new ("https://hub.spacestation14.com/api/servers")
    };
    
    protected override async void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        
        // Set our view from the "main" layout resource
        SetContentView(ResourceConstant.Layout.activity_main);
        
        Dependencies.InitializeUI(MainApp.ServiceCollection);
        Dependencies.Initialize(MainApp.ServiceCollection);

        FileService.RootPath = Application.Context.CacheDir!.AbsolutePath;
        
        MainApp.Build();
        _restService = MainApp.ServiceProvider.GetService<RestService>()!;

        await LoadServersList();
    }

    private async Task LoadServersList()
    {
        var servers = await LoadServers();
        
        var serverLayout = FindViewById<LinearLayout>(ResourceConstant.Id.server_layout);
        foreach (var server in servers)
        {
            serverLayout!.AddView(CreateServerView(server));
        }
    }

    private async Task<List<ServerInfo>> LoadServers()
    {
        return await _restService.GetAsyncDefault<List<ServerInfo>>(HubUris[0],[],CancellationToken.None);
    }

    private View CreateServerView(ServerInfo serverInfo)
    {
        var button = new Button(this)
        {
            Text =
                $"Name: {serverInfo.statusData.name} Online:{serverInfo.statusData.players}/{serverInfo.statusData.soft_max_players}"
        };

        button.Click += (_,_) => ProceedServer(serverInfo);
        
        return button;
    }

    private async void ProceedServer(ServerInfo serverInfo)
    {
        var serverLayout = FindViewById<LinearLayout>(ResourceConstant.Id.server_layout);
        serverLayout?.RemoveAllViews();

        var logger = (AndroidLogger)MainApp.ServiceProvider.GetService<ILogger>()!;
        logger.OnLog += s =>
        {
            mainHandler.Post(() =>
            {
                serverLayout.AddView(new TextView(this)
                {
                    Text = $"[LOG] {s}"
                });
            });
        };

        await Task.Run(async () =>
        {
            await RunGame((RobustUrl)serverInfo.address, MainApp.ServiceProvider);
        });

    }
    
    public async Task RunGame(RobustUrl url,IServiceProvider serviceProvider)
    {
        using (var cancelTokenSource = new CancellationTokenSource())
        {
            var contentService = serviceProvider.GetService<ContentService>()!;
            var authService = serviceProvider.GetService<AuthService>()!;
            var buildInfo = await contentService.GetBuildInfo(url!, cancelTokenSource.Token);
            var debug = serviceProvider.GetService<DebugService>()!;

            if (buildInfo.BuildInfo.auth.mode != "Disabled" && authService.CurrentLogin != null)
            {
                // var account = authService.CurrentLogin;
                // Environment.SetEnvironmentVariable("ROBUST_AUTH_TOKEN", account.Token.Token);
                // Environment.SetEnvironmentVariable("ROBUST_AUTH_USERID", account.UserId.ToString());
                // Environment.SetEnvironmentVariable("ROBUST_AUTH_PUBKEY", buildInfo.BuildInfo.auth.public_key);
                // Environment.SetEnvironmentVariable("ROBUST_AUTH_SERVER", "https://auth.spacestation14.com/");
            }
            
            var args = new List<string>
            {
                // Pass username to launched client.
                // We don't load username from client_config.toml when launched via launcher.
                "--username", authService.CurrentLogin?.Username ?? "Alise",

                // Tell game we are launcher
                "--cvar", "launch.launcher=true",
                "--cvar", "display.windowing_api=sdl2",
                "--cvar", "display.egl=true"
            };
            
            var connectionString = url.ToString();
            if (!string.IsNullOrEmpty(buildInfo.BuildInfo.connect_address))
                connectionString = buildInfo.BuildInfo.connect_address;
                
            // We are using the launcher. Don't show main menu etc..
            // Note: --launcher also implied --connect.
            // For this reason, content bundles do not set --launcher.
            args.Add("--launcher");

            args.Add("--connect-address");
            args.Add(connectionString);
                
            args.Add("--ss14-address");
            args.Add(url.ToString());
            
            try
            {
                var harm = new Harmony("ru.cinka");
                harm.PatchAll();
                await contentService.Run(args.ToArray(), buildInfo, cancelTokenSource.Token);
            }
            catch (Exception e)
            {
                PrintError(e,debug);
            }
        }
    }

    public void PrintError(Exception e, DebugService debug)
    {
        debug.Error(e.Message);
        debug.Error($"Source {e.Source}");
        debug.Error($"StackTrace {e.StackTrace}");
        
        if(e.InnerException is not null)
        {
            debug.Error("");
            PrintError(e.InnerException, debug);
        }
    }
    
}