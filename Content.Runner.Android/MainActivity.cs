using _Microsoft.Android.Resource.Designer;
using Android.Views;
using Content.Datum;
using Content.Datum.Data;
using Content.Datum.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Content.Runner.Android;

[Activity(Label = "@string/app_name", MainLauncher = true)]
public class MainActivity : Activity
{
    public ContentApp MainApp = new();

    private RestService _restService;
    
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
        
        MainApp.Build();
        _restService = MainApp.ServiceProvider.GetService<RestService>()!;
        
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
        var a = new LinearLayout(this);
        a.Orientation = Orientation.Horizontal;
        
        a.AddView(new TextView(this)
        {
            Text = $"Name: {serverInfo.statusData.name} Online:{serverInfo.statusData.players}/{serverInfo.statusData.soft_max_players}"
        });
        return a;
    }
}