namespace Content.Datum.Services;

public class ReflectionService
{
    private readonly AssemblyService _assemblyService;
    private readonly FileService _fileService;
    private readonly DebugService _debugService;
    public IEnumerable<string> TypePrefixes => _typePrefixes;

    // Cache these so that we only need to allocate the array ONCE.
    private static readonly string[] _typePrefixes = new[]
    {
        "",
        "Robust.Client.",
        "Robust.Shared.",
        "Content.Shared.",
        "Content.Client."
    };

    public ReflectionService(AssemblyService assemblyService, FileService fileService, DebugService debugService)
    {
        _assemblyService = assemblyService;
        _fileService = fileService;
        _debugService = debugService;
    }
    
    public Type? GetType(string name)
    {
        // The priority in which types are retrieved is based on the TypePrefixes list.
        // This is an implementation detail. If you need it: make a better API.
        foreach (string prefix in TypePrefixes)
        {
            string appendedName = prefix + name;
            foreach (var assembly in _assemblyService.Assemblies)
            {
                var theType = assembly.GetType(appendedName);
                if (theType != null)
                {
                    return theType;
                }
            }
        }

        return null;
    }
}