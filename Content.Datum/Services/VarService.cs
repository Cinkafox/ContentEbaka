namespace Content.Datum.Services;

public class VarService
{
    public readonly Uri EngineManifestUrl = new("https://central.spacestation14.io/builds/robust/manifest.json");
    public readonly Uri EngineModuleManifestUrl = new("https://central.spacestation14.io/builds/robust/modules.json");
    public readonly int ManifestDownloadProtocolVersion = 1;
    public readonly string RobustAssemblyName = "Robust.Client";
}