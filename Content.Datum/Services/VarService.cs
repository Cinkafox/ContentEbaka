namespace Content.Datum.Services;

public class VarService
{
    public readonly int ManifestDownloadProtocolVersion = 1;
    public readonly Uri EngineManifestUrl = new Uri("https://central.spacestation14.io/builds/robust/manifest.json");
    public readonly string RobustAssemblyName = "Robust.Client";
}