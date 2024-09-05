using System.Data;
using HarmonyLib;
using BindingFlags = System.Reflection.BindingFlags;

namespace Content.Script.Services;

public class HarmonyService(ReflectionService reflectionService)
{
    private HarmonyInstance? _instance;

    public HarmonyInstance Instance
    {
        get
        {
            if (_instance is null) throw new NoNullAllowedException();
            return _instance;
        }
    }

    public void CreateInstance()
    {
        if (_instance is not null) 
            throw new Exception();
        
        _instance = new HarmonyInstance();
        UnShittyWizard();
    }

    /// <summary>
    /// Я помню пенис большой,Я помню пенис большой, Я помню пенис большой, я помню....
    /// </summary>
    private void UnShittyWizard()
    {
        var method = reflectionService.GetType("Robust.Client.GameController").TypeInitializer;
        _instance!.Harmony.Patch(method, new HarmonyMethod(Prefix));
    }
    
    static bool Prefix()
    {
        // Returning false skips the execution of the original static constructor
        return false;
    }
}


public class HarmonyInstance
{
    public readonly Harmony Harmony;

    internal HarmonyInstance()
    {
        Harmony = new Harmony("ru.cinka.patch");
    }
}

