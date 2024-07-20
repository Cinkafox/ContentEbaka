namespace Content.Datum.Services;

public interface ILogger
{
    public void Log(LoggerCategory loggerCategory, string message);
}