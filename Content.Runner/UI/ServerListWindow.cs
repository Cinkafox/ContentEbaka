using System.Collections.ObjectModel;
using Content.Datum.Data;
using Content.Datum.Services;
using ContentDownloader.Data;
using Terminal.Gui;

namespace Content.Runner.UI;

public sealed class ServerListWindow : Window
{
    private readonly RestService _restService;

    public List<Uri> HubUris = new List<Uri>()
    { 
        new ("https://cdn.spacestationmultiverse.com/hub/api/servers"),
        new ("https://hub.spacestation14.com/api/servers")
    };

    public ServerListDataSource ServerListDataSource = new();
    
    private readonly ListView _serverListView;
    public ServerListWindow(RestService restService)
    {
        _restService = restService;
        _serverListView = new ListView()
        {
            Text = "Server list", Height = this.Height-2, Width = Dim.Fill()
        };
        var filterInput = new TextField()
        {
            Height = 2,
            Width = Dim.Percent(50), 
            Y = Pos.Bottom(_serverListView), 
            Border = { LineStyle = LineStyle.Heavy, Thickness = new Thickness(0,1,1,0) }
        };
        var exitButton = new Button()
        {
            Text = "EXIT",
            Y = Pos.Bottom(_serverListView) + 1,
            X = Pos.Right(filterInput) + 2,
        };

        var updateButton = new Button()
        {
            Text = "Update",
            Y = Pos.Bottom(_serverListView) + 1,
            X = Pos.Right(exitButton) + 2,
        };

        exitButton.MouseClick += (_,_) => RequestStop();
        updateButton.MouseClick += (_, _) => { };

        filterInput.TextChanging += FilterInputOnTextChanging;
        
        _serverListView.OpenSelectedItem += ServerListViewOnOpenSelectedItem;
        
        Add(_serverListView);
        Add(filterInput);
        Add(exitButton);
        Add(updateButton);
    }

    private void FilterInputOnTextChanging(object? sender, CancelEventArgs<string> e)
    {
        ServerListDataSource.ApplyFilter(e.NewValue);
    }

    private void ServerListViewOnOpenSelectedItem(object? sender, ListViewItemEventArgs e)
    {
        var item = (string)e.Value;
        Program.Url = new RobustUrl(item);
        RequestStop();
    }

    public async Task LoadData()
    {
        ServerListDataSource.Clear();
        var a = new ObservableCollection<string>()
        {
            "Loadint... Please wait"
        };
        
        await _serverListView.SetSourceAsync(a);
        foreach (var hubUrl in HubUris)
        {
            a.Add("Loading:" + hubUrl);
            await LoadData(hubUrl);
        }
        _serverListView.Source = ServerListDataSource;
    }

    public async Task LoadData(Uri hubUri)
    {
        var servers = await _restService.GetAsyncDefault<List<ServerInfo>>(hubUri,[],CancellationToken.None);
        ServerListDataSource.Append(servers);
    }
}