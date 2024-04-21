using System.Text;

namespace ContentDownloader.Services;

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