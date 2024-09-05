using System.Reflection;
using Content.Datum;
using Content.Datum.Services;
using HarmonyLib;

namespace Content.Script.Services;

public class PatchService(ReflectionService reflectionService, DebugService debugService, HarmonyService harmonyService)
{
    private MethodInfo? _ioCMethodInfo;

    public void Boot()
    {
        _ioCMethodInfo = reflectionService.GetType("Robust.Shared.IoC.IoCManager")!
            .GetMethod("Resolve", BindingFlags.Public | BindingFlags.Static, []);
        
        try
        {
            var m = reflectionService.GetType("Robust.Shared.IoC.IoCManager")!
                .GetMethod("BuildGraph");
            harmonyService.Instance.Harmony.Patch(m, null, new HarmonyMethod(OnBuildedGraph));
            debugService.Log("PATCHED SUCCESS");
        }
        catch (Exception ex)
        {
            debugService.StackTrace(ex);
        }
    }

    public (object, Type) IoC(string type)
    {
        var baseType = reflectionService.GetType(type);
        if (baseType is null) 
            throw new Exception();

        if (_ioCMethodInfo is null) 
            throw new Exception();

        var generic = _ioCMethodInfo.MakeGenericMethod(baseType);
        var obj = generic.Invoke(null,null);
        if (obj is null) throw new Exception();
        return (obj, baseType);
    }

    static void OnBuildedGraph()
    {
        ContentApp.Instance.GetService<ReflectionService>()
            .GetType("Robust.Shared.Log.Logger")!
            .GetMethod("Info", BindingFlags.Public | BindingFlags.Static, new []{typeof(string)})
            .Invoke(null, ["HEY! SOME SHIT IS ON! <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<"]);
    }
}