using System.Reflection;
using System.Runtime.InteropServices;
using Content.Datum.Services;
using Content.Runner.Android.Services;
using HarmonyLib;
using SharpZstd.Interop;

namespace Content.Runner.Android.HarmonyPatch;

[HarmonyLib.HarmonyPatch]
public class ZLibPatch
{
    [HarmonyTargetMethod]
    private static MethodBase TargetMethod()
    {
        return AccessTools.Method(typeof(AndroidLogger), nameof(AndroidLogger.Log));
    }

    [HarmonyPostfix]
    public static void Patch(LoggerCategory loggerCategory,ref string message)
    {
        message += " HI!";
    }
}