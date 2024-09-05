using Content.Datum;
using Content.Script;

namespace Content.Runner;

public static class Program
{
    public static void Main(string[] args)
    {
        new ContentApp()
            .RegisterDependencies<RunnerDependencies>()
            .RegisterDependencies<UIDependencies>()
            .RegisterDependencies<ScriptDependencies>()
            .Build()
            .Run(args);
    }
}