using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ContentDownloader.Data;
using ContentDownloader.Services;

namespace ContentUI;

public partial class MainWindow : Window
{
    public static Uri HUB_URL = new("https://hub.spacestation14.com/api/servers");
    
    private List<ServerInfo> _servers;

    public MainWindow()
    {
        InitializeComponent();
    }

    public async void LoadData()
    {
        _servers = await ConstServices.RestService.GetDataAsync<List<ServerInfo>>(HUB_URL) ?? [];
        Dispatcher.Invoke(UpdateData);
    }

    private void UpdateData()
    {
        
    }
}