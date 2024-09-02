
using System.Diagnostics;
using Content.Datum;
using Content.Datum.Data;
using Content.Datum.Data.FileApis;
using Content.Datum.Services;
using Content.Downloader.Services;
using Content.Downloader.UI;
using Content.UI.Console.UI;
using Microsoft.Extensions.DependencyInjection;
using Terminal.Gui;

namespace Content.Downloader;

public static class Program
{
    private static ContentApp _contentApp = default!;
    
    public static void Main(string[] args)
    {
        _contentApp = new ContentApp();
        
        Dependencies.Initialize(_contentApp.ServiceCollection);
        Dependencies.InitializeUI(_contentApp.ServiceCollection);
        
        _contentApp.Build();
        _contentApp.Run(args);
    }
}