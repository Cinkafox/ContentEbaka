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

    public AssemblyApi(IFileApi root)
    {
        _root = root;
    }
    
    public bool TryOpen(string path, out Stream? stream)
    {
        return _root.TryOpen(path, out stream);
    }

    public IEnumerable<string> AllFiles => _root.AllFiles;
}