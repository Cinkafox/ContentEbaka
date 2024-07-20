namespace Content.Datum.Services;

public class ConsoleLogger : ILogger
{
    public void Log(LoggerCategory loggerCategory, string message)
    {
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.Write($"[{Enum.GetName(loggerCategory)}] ");
        Console.ResetColor();
        Console.WriteLine(message);
    }
}