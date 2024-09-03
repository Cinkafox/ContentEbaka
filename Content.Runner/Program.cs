using Content.Datum;

namespace Content.Runner;

public static class Program
{
    private static ContentApp _contentApp = default!;

    public static void Main(string[] args)
    {
        _contentApp = new ContentApp();
        Dependencies.Initialize(_contentApp.ServiceCollection);
        Dependencies.InitializeUI(_contentApp.ServiceCollection);
        _contentApp.Build();
        _contentApp.Run(args);
    }
}