using Content.Datum.Services;

namespace Content.Runner.Android.Services;

public class AndroidLogger : ILogger
{
    public Action<string>? OnLog;
    
    public void Log(LoggerCategory loggerCategory, string message)
    {
        OnLog?.Invoke(message);
    }
}