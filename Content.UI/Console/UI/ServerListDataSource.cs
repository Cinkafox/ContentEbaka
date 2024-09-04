using System.Collections;
using System.Collections.Specialized;
using Content.Datum.Data;
using Terminal.Gui;
using Attribute = Terminal.Gui.Attribute;

namespace Content.UI.Console.UI;

public sealed class ServerListDataSource : IListDataSource
{
    private readonly List<string> _serverAddresses = new();
    private string? _currentFilter;
    private List<string>? _filteredAddresses;
    private readonly Attribute _selectedAttribute = new(Color.Black, Color.White);

    private readonly Dictionary<string, ServerInfo> _serverList = new();

    private bool _dirty = true;

    public List<string> FilteredAddresses
    {
        get
        {
            if (_filteredAddresses is not null) return _filteredAddresses;
            return _serverAddresses;
        }
    }

    public int Count => FilteredAddresses.Count;
    public int Length => FilteredAddresses.Count;
    public bool SuspendCollectionChangedEvent { get; set; }
    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    public bool IsMarked(int item)
    {
        return false;
    }

    public void Render(ListView container, ConsoleDriver driver, bool selected, int item, int col, int line, int width,
        int start = 0)
    {
        container.Move(col, line);

        var attr = driver.GetAttribute();

        if (selected) driver.SetAttribute(_selectedAttribute);

        var ip = FilteredAddresses[item];
        var serverInfo = _serverList[ip];

        var str =
            $"{Virovn(item.ToString(), Length.ToString().Length)}| {Virovn(serverInfo.statusData.name, 70)} {Virovn(serverInfo.statusData.players.ToString(), 4)}/{Virovn(serverInfo.statusData.soft_max_players.ToString(), 4)}";

        driver.AddStr(str);
        driver.SetAttribute(attr);
    }

    public void SetMark(int item, bool value)
    {
        throw new NotImplementedException();
    }

    public IList ToList()
    {
        if (_dirty)
        {
            FilteredAddresses.Sort((a, b) =>
            {
                var aa = _serverList[a];
                var bb = _serverList[b];

                var counta = aa.statusData.players;
                var countb = bb.statusData.players;

                return counta != countb 
                    ? countb.CompareTo(counta) 
                    : String.Compare(aa.statusData.name, bb.statusData.name, StringComparison.Ordinal);
            });
            _dirty = false;
        }
        return FilteredAddresses;
    }

    public void Dispose()
    {
        // TODO release managed resources here
    }

    public void Add(ServerInfo serverInfo)
    {
        if (!_serverList.TryGetValue(serverInfo.address, out _))
            _serverAddresses.Add(serverInfo.address);
        
        _serverList[serverInfo.address] = serverInfo;
        _dirty = true;
    }

    public void Append(IEnumerable<ServerInfo> serverInfos)
    {
        foreach (var serverInfo in serverInfos) Add(serverInfo);
    }

    public void Clear()
    {
        _serverList.Clear();
        _serverAddresses.Clear();
        _filteredAddresses = null;
        _currentFilter = null;
    }

    public void ApplyFilter(string? filter)
    {
        if (filter is not null)
        {
            _currentFilter = filter;
            _filteredAddresses = _serverAddresses.Where(Filter).ToList();
        }
        else
        {
            _currentFilter = null;
            _filteredAddresses = null;
        }
        _dirty = true;
    }

    private bool Filter(string address)
    {
        var info = _serverList[address];
        if (info.address.Contains(_currentFilter!)) return true;
        if (info.statusData.name.Contains(_currentFilter!)) return true;
        return false;
    }

    private string Virovn(string word, int max)
    {
        var col = word.Length;
        var len = max - col;
        var str = "";
        for (var i = 0; i < len; i++) str += " ";

        return word + str;
    }
}