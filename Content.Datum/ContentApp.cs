using Content.Datum.Data;
using Content.Datum.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Content.Datum;

public class ContentApp
{
    public static ContentApp Instance;
    
    private readonly IServiceCollection _serviceCollection = new ServiceCollection();
    private IServiceProvider _serviceProvider;

    public ContentApp()
    {
        RegisterDependencies<BaseDependencies>();
    }

    public ContentApp Build()
    {
        _serviceProvider = _serviceCollection.BuildServiceProvider();
        Instance = this;
        return this;
    }

    public ContentApp RegisterDependencies(IDependencyCollection dependencyCollection)
    {
        dependencyCollection.Register(_serviceCollection);
        return this;
    }

    public ContentApp RegisterDependencies<T>() where T : IDependencyCollection, new()
    {
        var t = new T();
        RegisterDependencies(t);
        return this;
    }

    public ContentApp Run(string[] args)
    {
        try
        {
            GetService<IExecutePoint>().Run(args);
        }
        catch (Exception e)
        { 
            GetService<DebugService>().StackTrace(e);
            Console.ReadKey();
        }
        return this;
    }

    public T GetService<T>()
    {
        return _serviceProvider.GetService<T>()!;
    }
}