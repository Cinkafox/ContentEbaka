namespace ContentDownloader.Services;

public static class ConstServices
{
    public static RestService RestService = new();
    public static EngineShit EngineShit = new();
    public static ILogger Logger = new Logger();
}

