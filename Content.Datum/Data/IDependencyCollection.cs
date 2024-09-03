using Microsoft.Extensions.DependencyInjection;

namespace Content.Datum.Data;

public interface IDependencyCollection
{
    public void Register(IServiceCollection serviceCollection);
}