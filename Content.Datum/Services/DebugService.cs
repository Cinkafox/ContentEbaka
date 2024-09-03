using System.Globalization;

namespace Content.Datum.Services;

public class DebugService : IDisposable
{
    public ILogger Logger;

    public static string LogPath = Path.Combine(FileService.RootPath, "log");
    public DateTime LogDate = DateTime.Now;
    public FileStream LogStream;
    public StreamWriter LogWriter;

    public DebugService(ILogger logger)
    {
        Logger = logger;
        
        if (!Directory.Exists(LogPath))
            Directory.CreateDirectory(LogPath);
        
        var filename = String.Format("{0:yyyy-MM-dd}.txt", DateTime.Now);

        LogStream = File.Open(Path.Combine(LogPath, filename),
            FileMode.Append, FileAccess.Write);
        LogWriter = new StreamWriter(LogStream);
    }

    public void Debug(string message)
    {
        Log(LoggerCategory.Debug, message);
    }

    public void Error(string message)
    {
        Log(LoggerCategory.Error, message);
    }

    public void Log(string message)
    {
        Log(LoggerCategory.Log, message);
    }

    public void Dispose()
    {
        LogWriter.Dispose();
        LogStream.Dispose();
    }

    private void Log(LoggerCategory category, string message)
    {
        Logger.Log(category, message);
        SaveToLog(category, message);
    }

    private void SaveToLog(LoggerCategory category, string message)
    {
        LogWriter.WriteLine($"[{category}] {message}");
        LogWriter.Flush();
    }
}

public enum LoggerCategory
{
    Log,
    Debug,
    Error
}