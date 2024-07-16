namespace Content.Datum.Services;

public class DebugService
{
    public ILogger Logger = new Logger();
    
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
    Log, Debug, Error
}

public interface ILogger
{
    public void Log(LoggerCategory loggerCategory, string message);
}

public class Logger : ILogger
{
    public void Log(LoggerCategory loggerCategory, string message)
    {
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.Write($"[{Enum.GetName(loggerCategory)}] ");
        Console.ResetColor();
        Console.WriteLine(message);
    }
}