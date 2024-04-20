namespace ContentDownloader;

public class RobustUrl
{
    public bool Secured { get; private set; }
    public string Scheme { get; private set; } = "http";
    public Uri Uri { get; private set; }
    public Uri HttpUri { get; private set; }
    public RobustPath InfoUri => new(this, "info");
    public RobustPath StatusUri => new(this, "status");
    public RobustUrl(string url)
    {
        Uri = new Uri(url);
        
        if (Uri.Scheme != "ss14" && Uri.Scheme != "ss14s") 
            throw new Exception("ss14 or ss14s only scheme");

        if (Uri.Scheme == "ss14s")
        {
            Secured = true;
            Scheme = "https";
        }

        HttpUri = Uri.Port != -1 ? 
            new Uri($"{Scheme}://{Uri.Host}:{Uri.Port}{Uri.PathAndQuery}") : 
            new Uri($"{Scheme}://{Uri.Host}{Uri.PathAndQuery}");
    }

    public override string ToString()
    {
        return HttpUri.ToString();
    }

    public static implicit operator Uri(RobustUrl url) => url.HttpUri;
    public static explicit operator RobustUrl(string url) => new(url);
    public static explicit operator RobustUrl(Uri uri) => new (uri.ToString());
}

public class RobustPath
{
    public RobustUrl Url;
    public string Path;
    
    public RobustPath(RobustUrl url, string path)
    {
        Url = url;
        Path = path;
    }

    public override string ToString()
    {
        return ((Uri)this).ToString();
    }
    
    public static implicit operator Uri(RobustPath path) => new (path.Url,path.Url.HttpUri.PathAndQuery + path.Path);
}