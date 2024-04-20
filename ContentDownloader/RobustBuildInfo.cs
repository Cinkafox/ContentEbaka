namespace ContentDownloader;

public class RobustBuildInfo
{
    public RobustUrl Url;
    public Info BuildInfo;
    public RobustManifestInfo RobustManifestInfo;
    public RobustBuildInfo(RobustUrl url)
    {
        Url = url;
        BuildInfo = ConstServices.RestService.GetDataSync<Info>(url.InfoUri);

        RobustManifestInfo = BuildInfo.build.acz ? 
            new RobustManifestInfo(new RobustPath(Url, "manifest.txt"), Url) : 
            new RobustManifestInfo(new Uri(BuildInfo.build.manifest_url),new Uri(BuildInfo.build.manifest_download_url));
        
    }
}