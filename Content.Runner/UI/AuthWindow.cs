using Content.Runner.Data.Auth;
using Content.Runner.Services;
using Terminal.Gui;

namespace Content.Runner.UI;

public sealed class AuthWindow : Window
{
    private readonly AuthService _authService;
    private readonly ServerListWindow _serverListWindow;
    private Button _submitButton;
    private TextField _passwordInput;
    private TextField _loginInput;

    public AuthWindow(AuthService authService, ServerListWindow serverListWindow)
    {
        _authService = authService;
        _serverListWindow = serverListWindow;

        _loginInput = new TextField()
        {
            Width = Dim.Fill(),
            Height = 1,
        };

        _passwordInput = new TextField()
        {
            Width = Dim.Fill(),
            Height = 1,
            Y = Pos.Bottom(_loginInput)
        };

        _submitButton = new Button()
        {
            Width = Dim.Fill(),
            Height = 1,
            Y = Pos.Bottom(_passwordInput),
            Text = "Auth"
        };
        
        _submitButton.MouseClick += ServerListWindowOnMouseClick;

        Add(_loginInput);
        Add(_passwordInput);
        Add(_submitButton);
    }

    private async void ServerListWindowOnMouseClick(object? sender, MouseEventEventArgs e)
    {
        _submitButton.Text = "Checking auth";
        if (await _authService.Auth(new AuthenticateRequest(_loginInput.Text, _passwordInput.Text)))
        {
            await Ensure();
        }
        _submitButton.Text = "Error!";
    }

    public async Task Ensure()
    {
        _submitButton.Text = "Please, wait";
        if (await _authService.EnsureToken())
        {
            _submitButton.Text = "Run some think";
            RequestStop();
            Application.Run(_serverListWindow);
            return;
        }
        _submitButton.Text = "Auth";
    }
}