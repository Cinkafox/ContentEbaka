using System.Reflection;
using ContentDownloader.Data;
using ContentDownloader.Services;
using ContentDownloader.Utils;

namespace ContentDownloader;

public static class Programm
{
    public static async Task Main(string[] args)
    {
        ConstServices.Logger.Log("Starting some shit");
        var a = new RobustBuildInfo((RobustUrl)"ss14://server.fishstation.ru");
        var cr = new ContentRunner(a);
        AssemblyHelper.RegisterInvoker("Robust.Client", assembly =>
        {
            ConstServices.Logger.Log("MEOW!");
            var sa = assembly.GetType("Robust.Client.Graphics.Clyde.Clyde");
            ConstServices.Logger.Log(sa.Name);
        });
        await cr.Run();
    }
}




public static class ConHelper{

    public static Assembly? FindAssembly(string type)
    {
        var splited = type.Split(".");
        var outp = "";
        foreach (var item in splited)
        {
            outp += item;
            if (AssemblyHelper.Assemblies.TryGetValue(outp, out var assembly))
                return assembly;
            outp += ".";
        }

        return null;
    }
    public static Type GetType(string type)
    {
        var splited = type.Split(".");
        var assemblyName = $"{splited[0]}.{splited[1]}";
        var assembly = AssemblyHelper.Assemblies[assemblyName];
        return assembly.GetType(type);
    }
}
