using Microsoft.Extensions.DependencyInjection;

namespace Content.Datum;

public class ContentApp
{
    public IServiceCollection ServiceCollection = new ServiceCollection();
    public IServiceProvider ServiceProvider;
    public ContentApp()
    {
        Dependency.Initialize(ServiceCollection);
    }

    public void Build()
    {
        ServiceProvider = ServiceCollection.BuildServiceProvider();
    }
}