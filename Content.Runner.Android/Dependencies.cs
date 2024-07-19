using Content.Runner.Android.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Content.Runner.Android;

public static class Dependencies
{
    public static void InitializeUI(IServiceCollection serviceCollection)
    {
     
    }
    public static void Initialize(IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<AuthService>();
    }
}