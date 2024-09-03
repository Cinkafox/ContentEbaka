using System.Net.Http.Headers;
using Content.Datum.Services;
using Content.Runner.Data.Auth;

namespace Content.Runner.Services;

public class AuthService
{
    private readonly HttpClient _httpClient = new();
    private readonly RestService _restService;

    public (string, string) AuthDatum = (string.Empty, string.Empty);

    public LoginInfo? CurrentLogin;

    public AuthService(RestService restService)
    {
        _restService = restService;
    }

    public async Task<bool> Auth(string login, string password)
    {
        var authUrl = new Uri("https://auth.spacestation14.com/api/auth/authenticate");

        var result =
            await _restService.PostAsync<AuthenticateResponse, AuthenticateRequest>(
                new AuthenticateRequest(login, password), authUrl, CancellationToken.None);
        if (result.Value is null) return false;
        CurrentLogin = new LoginInfo
        {
            Token = new LoginToken(result.Value.Token, result.Value.ExpireTime),
            UserId = result.Value.UserId
        };

        AuthDatum = (login, password);

        return true;
    }

    public async Task<bool> EnsureToken()
    {
        if (CurrentLogin is null) return false;

        var authUrl = new Uri("https://auth.spacestation14.com/api/auth/ping");

        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, authUrl);
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("SS14Auth", CurrentLogin.Token.Token);
        using var resp = await _httpClient.SendAsync(requestMessage);

        if (!resp.IsSuccessStatusCode) CurrentLogin = null;

        return resp.IsSuccessStatusCode;
    }
}