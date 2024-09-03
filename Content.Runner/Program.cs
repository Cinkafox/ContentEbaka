using Content.Datum;

namespace Content.Runner;

public static class Program
{
    public static void Main(string[] args)
    {
        new ContentApp()
            .RegisterDependencies<RunnerDependencies>()
            .RegisterDependencies<UIDependencies>()
            .Build()
            .Run(args);
    }
}