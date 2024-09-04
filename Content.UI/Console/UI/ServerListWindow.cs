using System.Collections.ObjectModel;
using Content.Datum.Data;
using Content.Datum.Services;
using Terminal.Gui;

namespace Content.UI.Console.UI;

public sealed class ServerListWindow : Window
{
    private readonly RestService _restService;
    private readonly ListView _serverListView;
    private readonly ServerListDataSource _serverListDataSource = new();
    private readonly List<Uri> _hubUris = new()
    {
        new("https://cdn.spacestationmultiverse.com/hub/api/servers"),
        new("https://hub.spacestation14.com/api/servers")
    };

    public string? SelectedUrl { get; private set; }
    public bool IsCurrentUpdateHub { get; private set; }

    public ServerListWindow(RestService restService)
    {
        _restService = restService;
        _serverListView = new ListView
        {
            Text = "Server list", Height = Height - 2, Width = Dim.Fill()
        };
        var filterInput = new TextField
        {
            Height = 2,
            Width = Dim.Percent(50),
            Y = Pos.Bottom(_serverListView),
            Border = { LineStyle = LineStyle.Heavy, Thickness = new Thickness(0, 1, 1, 0) }
        };
        var exitButton = new Button
        {
            Text = "EXIT",
            Y = Pos.Bottom(_serverListView) + 1,
            X = Pos.Right(filterInput) + 2
        };

        var updateButton = new Button
        {
            Text = "Update",
            Y = Pos.Bottom(_serverListView) + 1,
            X = Pos.Right(exitButton) + 2
        };

        exitButton.MouseClick += (_, _) => RequestStop();
        updateButton.MouseClick += (_, _) => UpdateHubs();

        filterInput.TextChanging += FilterInputOnTextChanging;

        _serverListView.OpenSelectedItem += ServerListViewOnOpenSelectedItem;

        Add(_serverListView);
        Add(filterInput);
        Add(exitButton);
        Add(updateButton);
    }

    private async void UpdateHubs()
    {
        if(IsCurrentUpdateHub) return;
        await LoadData();
    }

    private void FilterInputOnTextChanging(object? sender, CancelEventArgs<string> e)
    {
        _serverListDataSource.ApplyFilter(e.NewValue);
    }

    private void ServerListViewOnOpenSelectedItem(object? sender, ListViewItemEventArgs e)
    {
        var item = (string)e.Value;
        SelectedUrl = item;
        RequestStop();
    }

    public async Task LoadData()
    {
        IsCurrentUpdateHub = true;
        _serverListDataSource.Clear();
        var a = new ObservableCollection<string>
        {
            "Loading... Please wait"
        };

        await _serverListView.SetSourceAsync(a);
        foreach (var hubUrl in _hubUris)
        {
            a.Add("Loading:" + hubUrl);
            await LoadData(hubUrl);
        }

        _serverListView.Source = _serverListDataSource;
        IsCurrentUpdateHub = false;
    }

    public async Task LoadData(Uri hubUri)
    {
        var servers = await _restService.GetAsyncDefault<List<ServerInfo>>(hubUri, [], CancellationToken.None);
        _serverListDataSource.Append(servers);
    }
}