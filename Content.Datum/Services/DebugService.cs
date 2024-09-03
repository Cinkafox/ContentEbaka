namespace Content.Datum.Services;

public class DebugService
{
    public ILogger Logger;

    public DebugService(ILogger logger)
    {
        Logger = logger;
    }

    public void Debug(string message)
    {
        Logger.Log(LoggerCategory.Debug, message);
    }

    public void Error(string message)
    {
        Logger.Log(LoggerCategory.Error, message);
    }

    public void Log(string message)
    {
        Logger.Log(LoggerCategory.Log, message);
    }
}

public enum LoggerCategory
{
    Log,
    Debug,
    Error
}