using System.Text;

namespace ContentDownloader.Services;

public interface ILogger
{
    public void Log(params object[] objects);
    public delegate void Logging(object sender, string log);
    public event Logging? OnLog;
}

public class Logger : ILogger
{
    public event ILogger.Logging? OnLog;
    
    public void Log(params object[] objects)
    {
        var str = new StringBuilder();
        foreach (var obj in objects)
        {
            str.Append(" " + obj);
        }
        
        OnLog?.Invoke(this,str.ToString());

        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.Write("[LOG]");
        Console.ResetColor();
        Console.WriteLine(str);
    }
}