using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Diagnostics;
using Content.Datum.Data;
using Content.Datum.Data.FileApis.Interfaces;
using Content.Datum.Services;
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
    FileService fileService,
    ContentService contentService)
    : IExecutePoint, IRedialApi
{
    private readonly IReadWriteFileApi _authFileApi = fileService.CreateFileApi("/auth");
    private ApplicationOption _option = default!;

    public void Run(string[] args)
    {
        _option = new ApplicationOption(args);
        Application.QuitKey = Key.C.WithCtrl;
        Task.Run(RunAsync).Wait();
    }

    public void Redial(Uri uri, string text = "")
    {
        _option.Url = uri.ToString();
        debugService.Debug("Redirect " + uri);
        StartProcess();
    }

    private async Task RunAsync()
    {
        if (string.IsNullOrWhiteSpace(_option.Login))
        {
            var successReadAuthDat = await ReadAuthDat();
            debugService.Debug("Reading auth data");

            if (!successReadAuthDat)
            {
                debugService.Debug("Auth is require.");

                do
                {
                    Application.Init();
                    Application.Run(authWindow);
                    Application.Shutdown();
                    debugService.Debug("Trying to auth...");
                } while (await authService.Auth(authWindow.Login, authWindow.Password));

                debugService.Debug("Success! Going to menu");
                await SaveAuthDat(authWindow.Login, authWindow.Password);
            }

            _option.Login = authService.AuthDatum.Item1;
            _option.Password = authService.AuthDatum.Item2;
        }

        if (string.IsNullOrWhiteSpace(_option.Url))
        {
            RunUi();
            return;
        }

        await RunGame();
    }

    private void RunUi()
    {
        Task.Run(serverListWindow.LoadData);
        Application.Init();
        Application.Run(serverListWindow);
        Application.Shutdown();

        if (serverListWindow.SelectedUrl is null)
        {
            debugService.Debug("Nothing to do now! Exit!");
            return;
        }

        _option.Url = serverListWindow.SelectedUrl;

        StartProcess();
    }

    private async Task<bool> ReadAuthDat()
    {
        if (!_authFileApi.TryOpen("auth.dat", out var stream))
            return false;

        string? login;
        string? password;

        using (var sw = new StreamReader(stream))
        {
            login = await sw.ReadLineAsync();
            password = await sw.ReadLineAsync();
            if (login is null || password is null) return false;
        }

        stream.Close();

        return await authService.Auth(login, password);
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

    private async Task RunGame()
    {
        var url = new RobustUrl(_option.Url);

        using var cancelTokenSource = new CancellationTokenSource();
        var buildInfo = await contentService.GetBuildInfo(url, cancelTokenSource.Token);

        if (buildInfo.BuildInfo.auth.mode != "Disabled")
        {
            var successAuth = await authService.Auth(_option.Login, _option.Password);
            if (!successAuth)
            {
                debugService.Debug("Failed to auth with: " + _option.Login + " " + _option.Password);
                return;
            }

            var account = authService.CurrentLogin!;
            Environment.SetEnvironmentVariable("ROBUST_AUTH_TOKEN", account.Token.Token);
            Environment.SetEnvironmentVariable("ROBUST_AUTH_USERID", account.UserId.ToString());
            Environment.SetEnvironmentVariable("ROBUST_AUTH_PUBKEY", buildInfo.BuildInfo.auth.public_key);
            Environment.SetEnvironmentVariable("ROBUST_AUTH_SERVER", "https://auth.spacestation14.com/");
        }

        var args = new List<string>
        {
            // Pass username to launched client.
            // We don't load username from client_config.toml when launched via launcher.
            "--username", string.IsNullOrEmpty(_option.Login) ? "Alice" : _option.Login,

            // Tell game we are launcher
            "--cvar", "launch.launcher=true"
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

        await runnerService.Run(args.ToArray(), buildInfo, this, cancelTokenSource.Token);
    }

    private void StartProcess()
    {
        var info = new ProcessStartInfo(Environment.ProcessPath!, _option.Args)
        {
            UseShellExecute = true
        };

        Process.Start(info);
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

    public string[] Args =>
    [
        "--login", Login, "--password", Password, "--url", Url
    ];

    private void Handler(string login, string password, string url)
    {
        Login = login;
        Password = password;
        Url = url;
    }
}