using Content.Datum.Data;
using Content.Datum.Utils;
using ContentDownloader.Utils;

namespace ContentDownloader.Data;

public class RobustBuildInfo
{
    public RobustUrl Url;
    public Info BuildInfo;
    public RobustManifestInfo RobustManifestInfo;
}