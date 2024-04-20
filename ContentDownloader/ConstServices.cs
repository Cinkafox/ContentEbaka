using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace ContentDownloader;

public static class ConstServices
{
    public static RestService RestService = new();
    public static EngineShit EngineShit = new();
    public static ILogger Logger = new Logger();
}

public interface ILogger
{
    public void Log(params object[] objects);
}

public class Logger : ILogger
{
    public void Log(params object[] objects)
    {
        var str = new StringBuilder();
        foreach (var obj in objects)
        {
            str.Append(" " + obj);
        }

        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.Write("[LOG]");
        Console.ResetColor();
        Console.WriteLine(str);
    }
}


public class RestService
{
    public readonly HttpClient Client = new();
    JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };
    
    public async Task<T?> GetDataAsync<T>(Uri uri)
    {
        try
        {
            var response = await Client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(content, _serializerOptions);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(@"\tERROR {0}", ex.Message);
        }

        return default!;
    }

    public HttpResponseMessage GetSync(Uri uri)
    {
        var tast = Task.Run(() => Client.GetAsync(uri));
        tast.Wait();
        return tast.Result;
    }
    

    public static string ReadAsStringSync(HttpContent content)
    {
        var task2 = Task.Run(content.ReadAsStringAsync);
        task2.Wait();
                
        return task2.Result;
    }

    public T GetDataSync<T>(Uri uri)
    {
        try
        {
            var response = GetSync(uri);
            if (response.IsSuccessStatusCode)
            {
                var content = ReadAsStringSync(response.Content);
                return JsonSerializer.Deserialize<T>(content, _serializerOptions);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(@"\tERROR {0}", ex.Message);
        }

        return default!;
    }
}