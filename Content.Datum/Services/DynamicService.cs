using System.Reflection;
using System.Reflection.Emit;
using Robust.Shared.IoC;

namespace Content.Datum.Services;

public class DynamicService
{
    private readonly ReflectionService _reflectionService;
    private readonly DebugService _debugService;

    AssemblyName assemblyName = new AssemblyName("MyDynamicAssembly");
    AssemblyBuilder assemblyBuilder;
    ModuleBuilder moduleBuilder;
    
    public DynamicService(ReflectionService reflectionService, ContentService contentService, DebugService debugService)
    {
        _reflectionService = reflectionService;
        _debugService = debugService;
        assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(
            assemblyName,
            AssemblyBuilderAccess.Run);
        moduleBuilder
            = assemblyBuilder.DefineDynamicModule("MyDynamicModule");
        _debugService.Debug("ASASAS");
    }

}

