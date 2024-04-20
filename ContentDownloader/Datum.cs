namespace ContentDownloader;

public class Auth
{
    public string mode { get; set; }
    public string public_key { get; set; }
}

public class Build
{
    public string engine_version { get; set; }
    public string fork_id { get; set; }
    public string version { get; set; }
    public string download_url { get; set; }
    public string manifest_download_url { get; set; }
    public string manifest_url { get; set; }
    public bool acz { get; set; }
    public string hash { get; set; }
    public string manifest_hash { get; set; }
}

public class Link
{
    public string name { get; set; }
    public string icon { get; set; }
    public string url { get; set; }
}

public class Info
{
    public string connect_address { get; set; }
    public Auth auth { get; set; }
    public Build build { get; set; }
    public string desc { get; set; }
    public List<Link> links { get; set; }
}

public class Status
{
    public string name { get; set; }
    public int players { get; set; }
    public List<object> tags { get; set; }
    public string map { get; set; }
    public int round_id { get; set; }
    public int soft_max_players { get; set; }
    public bool panic_bunker { get; set; }
    public int run_level { get; set; }
    public string preset { get; set; }
}

public enum ContentCompressionScheme
{
    None = 0,
    Deflate = 1,

    /// <summary>
    /// ZStandard compression. In the future may use SS14 specific dictionary IDs in the frame header.
    /// </summary>
    ZStd = 2,
}