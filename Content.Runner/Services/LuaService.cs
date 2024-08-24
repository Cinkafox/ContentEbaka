using System.Reflection;
using Content.Datum.Services;
using Newtonsoft.Json;
using NLua;

namespace Content.Runner.Services;

public class ScriptService
{
    private readonly FileService _fileService;
    private readonly ReflectionService _reflectionService;
    private HashSet<LoadedScript> _loadedScripts = new();

    public static string LuaScriptName = "script.lua";
    public static string ManifestName = "manifest.json";

    public ScriptService(FileService fileService, ContentService contentService, ReflectionService reflectionService)
    {
        _fileService = fileService;
        _reflectionService = reflectionService;
    }
    
    
    private void ReadScript(string path)
    {
        var lua = new Lua();
        lua.DoFile(Path.Combine(path,LuaScriptName));
        var manifest = ReadScriptManifest(Path.Combine(path,ManifestName));
    }

    private ScriptManifest ReadScriptManifest(string path)
    {
        using StreamReader r = new StreamReader(path);
        var json = r.ReadToEnd();
        var manifest = JsonConvert.DeserializeObject<ScriptManifest>(json);
        if (manifest is null)
            throw new ArgumentNullException(nameof(path));

        return manifest;
    }
}

public sealed class InjectItem
{
    public string Type { get; set; } = default!;
}

public sealed class ScriptManifest
{
    public Dictionary<string, InjectItem>? Inject { get; set; }
    public string InitValue { get; set; } = "Initialize";
    public string UpdateValue { get; set; } = "Update";
}

public interface ILuaProvider
{
    public Lua Lua { get; set; }
}

public class LoadedScript : ILuaProvider
{
    public ScriptManifest Manifest { get; set; }
    public Lua Lua { get; set; }
    public LuaFunction? InitFunc { get; set; }
    public LuaFunction? UpdateFunc { get; set; }

    public LoadedScript(Lua lua, LuaFunction? initFunc, LuaFunction? updateFunc)
    {
        Lua = lua;
        InitFunc = initFunc;
        UpdateFunc = updateFunc;
    }
}