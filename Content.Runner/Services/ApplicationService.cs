using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Diagnostics;
using Content.Datum.Data;
using Content.Datum.Data.FileApis.Interfaces;
using Content.Datum.Services;
using Content.Runner.Data.Auth;
using Content.Runner.UI;
using Content.UI.Console.UI;
using Robust.LoaderApi;
using Terminal.Gui;

namespace Content.Runner.Services;

public class ApplicationService(
    DebugService debugService,
    AuthService authService,
    RunnerService runnerService,
    AuthWindow authWindow,
    ServerListWindow serverListWindow,
    FileService fileService)
    : IExecutePoint
{
    private readonly DebugService _debugService = debugService;
    private readonly AuthService _authService = authService;
    private readonly RunnerService _runnerService = runnerService;
    private readonly AuthWindow _authWindow = authWindow;
    private readonly ServerListWindow _serverListWindow = serverListWindow;
    private readonly FileService _fileService = fileService;
    private readonly IReadWriteFileApi _authFileApi = fileService.CreateFileApi("/auth");
    private ApplicationOption _option = default!;

    public void Run(string[] args)
    {
        _option = new ApplicationOption(args);
        Application.QuitKey = Key.C.WithCtrl;
        Task.Run(RunAsync).Wait();
    }

    private async Task RunAsync()
    {
        if (string.IsNullOrWhiteSpace(_option.Login))
        {
            var successReadAuthDat = await ReadAuthDat();
            _debugService.Debug("Reading auth data");
        
            if (!successReadAuthDat)
            {
                _debugService.Debug("Auth is require.");
            
                do
                {
                    Application.Init();
                    Application.Run(_authWindow);
                    Application.Shutdown();
                    _debugService.Debug("Trying to auth...");

                } while (await _authService.Auth(_authWindow.Login, _authWindow.Password));
            
                _debugService.Debug("Success! Going to menu");
                await SaveAuthDat(_authWindow.Login, _authWindow.Password);
            }

            _option.Login = _authService.AuthDatum.Item1;
            _option.Password = _authService.AuthDatum.Item2;
        }
        
        if (string.IsNullOrWhiteSpace(_option.Url))
        {
            RunUi();
            return;
        }

        await RunClient();
    }

    private void RunUi()
    {
        Task.Run(_serverListWindow.LoadData);
        Application.Init();
        Application.Run(_serverListWindow);
        Application.Shutdown();

        if (_serverListWindow.SelectedUrl is null)
        {
            _debugService.Debug("Nothing to do now! Exit!");
            return;
        }

        _option.Url = _serverListWindow.SelectedUrl;

        List<string> args = new List<string>();
        args.Add("--login");
        args.Add(_option.Login);
        args.Add("--password");
        args.Add(_option.Password);
        args.Add("--url");
        args.Add(_option.Url);

        var info = new ProcessStartInfo(Environment.ProcessPath!, args)
        {
            UseShellExecute = true
        };

        Process.Start(info);
    }

    private async Task<bool> ReadAuthDat()
    {
        if (_authFileApi.TryOpen("auth.dat", out var stream))
        {
            string? login;
            string? password;
            
            using (var sw = new StreamReader(stream))
            {
                login = await sw.ReadLineAsync();
                password = await sw.ReadLineAsync();
                if(login is null || password is null) return false;
            }
            stream.Close();

            return await _authService.Auth(login, password);
        }

        return false;
    }

    private async Task SaveAuthDat(string login, string password)
    {
        using var ms = new MemoryStream();
        await using var sw = new StreamWriter(ms);
        await sw.WriteLineAsync(login);
        await sw.WriteLineAsync(password);
        await sw.FlushAsync();
        ms.Seek(0, SeekOrigin.Begin);
        _authFileApi.Save("auth.dat", ms);
    }
    
    private async Task RunClient()
    {
        var successAuth = await _authService.Auth(_option.Login, _option.Password);
        if (!successAuth)
        {
            _debugService.Debug("Failed to auth with: " + _option.Login + " " + _option.Password);
            return;
        }

        await _runnerService.RunGame(new RobustUrl(_option.Url));
    }
}

public class ApplicationOption
{
    public static readonly Option<string> LoginOption = new("--login");
    public static readonly Option<string> PasswordOption = new("--password");
    public static readonly Option<string> UrlOption = new("--url");

    public string Login = string.Empty;
    public string Password = string.Empty;
    public string Url = string.Empty;

    public ApplicationOption(string[] args)
    {
        var root = new RootCommand("run ss14 content")
        {
            LoginOption,
            PasswordOption,
            UrlOption
        };

        root.SetHandler(Handler, LoginOption, PasswordOption, UrlOption);

        var commandLineBuilder = new CommandLineBuilder(root);
            
        commandLineBuilder.UseDefaults();
        var parser = commandLineBuilder.Build();
        parser.Invoke(args);
    }

    private void Handler(string login, string password, string url)
    {
        Login = login;
        Password = password;
        Url = url;
    }
}


