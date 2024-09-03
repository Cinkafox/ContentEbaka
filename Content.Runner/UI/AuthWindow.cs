using Terminal.Gui;

namespace Content.Runner.UI;

public sealed class AuthWindow : Window
{
    private readonly TextField _loginInput;
    private readonly TextField _passwordInput;
    private readonly Button _submitButton;

    public string Login = string.Empty;
    public string Password = string.Empty;

    public AuthWindow()
    {
        _loginInput = new TextField
        {
            Width = Dim.Fill(),
            Height = 1
        };

        _passwordInput = new TextField
        {
            Width = Dim.Fill(),
            Height = 1,
            Y = Pos.Bottom(_loginInput)
        };

        _submitButton = new Button
        {
            Width = Dim.Fill(),
            Height = 1,
            Y = Pos.Bottom(_passwordInput),
            Text = "Auth"
        };

        _submitButton.MouseClick += OnSubmitClick;

        Add(_loginInput);
        Add(_passwordInput);
        Add(_submitButton);
    }

    private void OnSubmitClick(object? sender, MouseEventEventArgs e)
    {
        Login = _loginInput.Text;
        Password = _passwordInput.Text;
        RequestStop();
    }
}