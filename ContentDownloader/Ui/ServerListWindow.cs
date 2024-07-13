using ContentDownloader.Data;
using ContentDownloader.Services;
using Terminal.Gui;
using Attribute = Terminal.Gui.Attribute;

namespace ContentDownloader.Ui;

public sealed class ServerListWindow : Window
{
    public static Uri HUB_URL = new("https://hub.spacestation14.com/api/servers");
    private readonly ListView _serverListView;
    private List<ServerInfo> _servers = new();
    public ServerListWindow()
    {
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

        exitButton.MouseClick += (_,_) => RequestStop();
        
        filterInput.TextChanging += FilterInputOnTextChanging;
        
        _serverListView.SetSource(_servers);
        _serverListView.RowRender += ServerListViewOnRowRender;
        _serverListView.OpenSelectedItem += ServerListViewOnOpenSelectedItem;
        
        Add(_serverListView);
        Add(filterInput);
        Add(exitButton);
    }

    private void ServerListViewOnOpenSelectedItem(object? sender, ListViewItemEventArgs e)
    {
        var item = (ServerInfo)e.Value;
        Programm.Url = new RobustUrl(item.address);
        RequestStop();
    }

    private void FilterInputOnTextChanging(object? sender, StateEventArgs<string> e)
    {
        if (string.IsNullOrEmpty(e.NewValue))
        {
            SetData(_servers);
            return;
        }
        
        SetData(_servers.Where(info => info.statusData.name.Contains(e.NewValue)).ToList());
    }

    private void ServerListViewOnRowRender(object? sender, ListViewRowEventArgs obj)
    {
        if (obj.Row == _serverListView.SelectedItem) {
            obj.RowAttribute = new Attribute(Color.Black, Color.White);
        }
    }

    public async Task LoadData()
    {
        _servers = await ConstServices.RestService.GetDataAsync<List<ServerInfo>>(HUB_URL) ?? [];
        SetData(_servers);
    }

    public void SetData<T>(List<T> data)
    {
        _serverListView.SetSource(data);
        _serverListView.SelectedItem = 0;
    }
}