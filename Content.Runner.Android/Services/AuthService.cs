using System.Net.Http.Headers;
using Content.Datum.Data.FileApis.Interfaces;
using Content.Datum.Services;
using Content.Runner.Data.Auth;

namespace Content.Runner.Android.Services;

public class AuthService
{
    private readonly RestService _restService;
    private readonly FileService _fileService;
    private readonly HttpClient _httpClient = new HttpClient();
    
    public LoginInfo? CurrentLogin;
    public IReadWriteFileApi AuthFileApi;

    public AuthService(RestService restService, FileService fileService)
    {
        _restService = restService;
        _fileService = fileService;
        AuthFileApi = fileService.CreateFileApi("/auth");

        if (AuthFileApi.TryOpen("auth.dat", out var stream))
        {
            string? login;
            string? password;
            
            using (var sw = new StreamReader(stream))
            {
                login = sw.ReadLine();
                password = sw.ReadLine();
                if(login is null || password is null) return;
            }
            stream.Close();
            
            var task = Task.Run(() => Auth(new AuthenticateRequest(login, password)));
            task.Wait();
        }
    }

    public async Task<bool> Auth(AuthenticateRequest request)
    {
        var authUrl = new Uri("https://auth.spacestation14.com/api/auth/authenticate");

        var result = await _restService.PostAsync<AuthenticateResponse, AuthenticateRequest>(request, authUrl, CancellationToken.None);
        if (result.Value is null) return false;
        CurrentLogin = new LoginInfo()
        {
            Token = new LoginToken(result.Value.Token, result.Value.ExpireTime),
            UserId = result.Value.UserId
        };

        using var ms = new MemoryStream();
        await using var sw = new StreamWriter(ms);
        await sw.WriteLineAsync(request.Username);
        await sw.WriteLineAsync(request.Password);
        await sw.FlushAsync();
        ms.Seek(0, SeekOrigin.Begin);
        
        AuthFileApi.Save("auth.dat", ms);

        return true;
    }

    public async Task<bool> EnsureToken()
    {
        if (CurrentLogin is null) return false;
        
        var authUrl = new Uri("https://auth.spacestation14.com/api/auth/ping");

        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, authUrl);
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("SS14Auth", CurrentLogin.Token.Token);
        using var resp = await _httpClient.SendAsync(requestMessage);

        if (!resp.IsSuccessStatusCode)
        {
            CurrentLogin = null;
        }
        
        return resp.IsSuccessStatusCode;
    }
}