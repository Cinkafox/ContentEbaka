using ContentDownloader.Services;
using ContentDownloader.Utils;

namespace ContentDownloader.Data;

public class RobustBuildInfo
{
    public RobustUrl Url;
    public Info BuildInfo;
    public RobustManifestInfo RobustManifestInfo;
    public RobustBuildInfo(RobustUrl url)
    {
        ConstServices.Logger.Log("Fetching info from " + url);
        Url = url;
        BuildInfo = ConstServices.RestService.GetDataSync<Info>(url.InfoUri);

        RobustManifestInfo = BuildInfo.build.acz ? 
            new RobustManifestInfo(new RobustPath(Url, "manifest.txt"), new RobustPath(Url, "download")) : 
            new RobustManifestInfo(new Uri(BuildInfo.build.manifest_url),new Uri(BuildInfo.build.manifest_download_url));
        
    }
}