using Content.Datum.Data;
using Microsoft.Extensions.DependencyInjection;

namespace Content.Datum;

public class ContentApp
{
    private readonly IServiceCollection _serviceCollection = new ServiceCollection();
    private IServiceProvider _serviceProvider;

    public ContentApp()
    {
        RegisterDependencies<BaseDependencies>();
    }

    public ContentApp Build()
    {
        _serviceProvider = _serviceCollection.BuildServiceProvider();
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
        _serviceProvider.GetService<IExecutePoint>()?.Run(args);
        return this;
    }
}