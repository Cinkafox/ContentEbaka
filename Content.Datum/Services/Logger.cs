namespace Content.Datum.Services;

public class ConsoleLogger : ILogger
{
    public void Log(LoggerCategory loggerCategory, string message)
    {
        Console.ForegroundColor = loggerCategory switch
        {
            LoggerCategory.Log => ConsoleColor.DarkCyan,
            LoggerCategory.Debug => ConsoleColor.Blue,
            LoggerCategory.Error => ConsoleColor.Red,
            LoggerCategory.Fatal => ConsoleColor.DarkRed,
            _ => throw new ArgumentOutOfRangeException(nameof(loggerCategory), loggerCategory, null)
        };

        Console.Write($"[{Enum.GetName(loggerCategory)}] ");
        Console.ResetColor();
        Console.WriteLine(message);
    }
}