using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using ContentDownloader.Services;
using Robust.LoaderApi;

namespace ContentDownloader.Utils;

public class AssemblyHelper
{
    public static Dictionary<string, Assembly> Assemblies = new();
    public static event Action<string,Assembly>? OnAssemblyLoaded;
    public const string RobustAssemblyName = "Robust.Client";

    public readonly IFileApi FileApi;

    public AssemblyHelper(IFileApi fileApi)
    {
        AssemblyLoadContext.Default.Resolving += LoadContextOnResolving;
        AssemblyLoadContext.Default.ResolvingUnmanagedDll += LoadContextOnResolvingUnmanaged;
        FileApi = fileApi;
    }

    public static void RegisterInvoker(string assemblyName, Action<Assembly> func)
    {
        ConstServices.Logger.Log("Registering",assemblyName);
        OnAssemblyLoaded += (s, assembly) =>
        {
            if (s == assemblyName) func(assembly);
        };
    }
    
    public IntPtr LoadContextOnResolvingUnmanaged(Assembly assembly, string unmanaged)
    {
        var ourDir = Path.GetDirectoryName(typeof(AssemblyHelper).Assembly.Location);
        var a = Path.Combine(ourDir!, unmanaged);
        if (NativeLibrary.TryLoad(a, out var handle))
            return handle;

        return IntPtr.Zero;
    }
    

    public static bool TryGetLoader(Assembly clientAssembly, [NotNullWhen(true)] out ILoaderEntryPoint? loader)
    {
        loader = null;
        // Find ILoaderEntryPoint with the LoaderEntryPointAttribute
        var attrib = clientAssembly.GetCustomAttribute<LoaderEntryPointAttribute>();
        if (attrib == null)
        {
            Console.WriteLine("No LoaderEntryPointAttribute found on Robust.Client assembly!");
            return false;
        }

        var type = attrib.LoaderEntryPointType;
        if (!type.IsAssignableTo(typeof(ILoaderEntryPoint)))
        {
            Console.WriteLine("Loader type '{0}' does not implement ILoaderEntryPoint!", type);
            return false;
        }

        loader = (ILoaderEntryPoint) Activator.CreateInstance(type)!;
        return true;
    }

    public Assembly? LoadContextOnResolving(AssemblyLoadContext arg1, AssemblyName arg2)
    {
        return TryOpenAssembly(arg2.Name!, out var assembly) ? assembly : null;
    }

    public bool TryOpenAssembly(string name, [NotNullWhen(true)] out Assembly? assembly)
    {
        if (!TryOpenAssemblyStream(name, out var asm, out var pdb))
        {
            assembly = null;
            return false;
        }

        assembly = AssemblyLoadContext.Default.LoadFromStream(asm, pdb);
        ConstServices.Logger.Log("LOADED ASSEMBLY " + name);
        Assemblies[name] = assembly;
        OnAssemblyLoaded?.Invoke(name,assembly);
        return true;
    }

    public bool TryOpenAssemblyStream(string name, [NotNullWhen(true)] out Stream? asm, out Stream? pdb)
    {
        asm = null;
        pdb = null;

        if (!FileApi.TryOpen($"{name}.dll", out asm))
            return false;

        FileApi.TryOpen($"{name}.pdb", out pdb);
        return true;
    }
}
