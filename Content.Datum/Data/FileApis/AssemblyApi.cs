using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using Content.Datum.Services;
using Microsoft.Extensions.DependencyInjection;
using Robust.LoaderApi;

namespace Content.Datum.Data.FileApis;

public class AssemblyApi : IFileApi
{
    private readonly IFileApi _root;
    private readonly DebugService _debugService;

    private static Dictionary<string, Assembly> AssemblyCache = new();

    public AssemblyApi(IFileApi root, IServiceProvider serviceProvider)
    {
        _root = root;
        _debugService = serviceProvider.GetService<DebugService>()!;
        
        AssemblyLoadContext.Default.Resolving += LoadContextOnResolving;
        AssemblyLoadContext.Default.ResolvingUnmanagedDll += LoadContextOnResolvingUnmanaged;
    }
    
    public bool TryOpen(string path, out Stream? stream)
    {
        return _root.TryOpen(path, out stream);
    }

    public IEnumerable<string> AllFiles => _root.AllFiles;
    
    public bool TryGetLoader(Assembly clientAssembly, [NotNullWhen(true)] out ILoaderEntryPoint? loader)
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

    public bool TryOpenAssembly(string name, [NotNullWhen(true)] out Assembly? assembly)
    {
        if (AssemblyCache.TryGetValue(name, out assembly))
        {
            return true;
        }
        
        if (!TryOpenAssemblyStream(name, out var asm, out var pdb))
        {
            assembly = null;
            return false;
        }

        assembly = AssemblyLoadContext.Default.LoadFromStream(asm, pdb);
        AssemblyCache[name] = assembly;
        _debugService.Log("LOADED ASSEMBLY " + name);
        asm.Dispose();
        pdb?.Dispose();
        return true;
    }

    public bool TryOpenAssemblyStream(string name, [NotNullWhen(true)] out Stream? asm, out Stream? pdb)
    {
        asm = null;
        pdb = null;

        if (!_root.TryOpen($"{name}.dll", out asm))
            return false;

        _root.TryOpen($"{name}.pdb", out pdb);
        return true;
    }
    
    private IntPtr LoadContextOnResolvingUnmanaged(Assembly assembly, string unmanaged)
    {
        var ourDir = Path.GetDirectoryName(typeof(AssemblyApi).Assembly.Location);
        var a = Path.Combine(ourDir!, unmanaged);
        
        _debugService.Debug($"Loading dll lib: {a}");
        
        if (NativeLibrary.TryLoad(a, out var handle))
            return handle;

        return IntPtr.Zero;
    }
    
    private Assembly? LoadContextOnResolving(AssemblyLoadContext arg1, AssemblyName arg2)
    {
        _debugService.Debug("Resolving assembly from FileAPI: " + arg2.Name);
        return TryOpenAssembly(arg2.Name!, out var assembly) ? assembly : null;
    }
}