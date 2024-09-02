using ContentDownloader.Utils;

namespace Content.Datum.Data;

public class RobustUrl
{
    public Uri Uri { get; private set; }
    public Uri HttpUri { get; private set; }
    public RobustPath InfoUri => new(this, "info");
    public RobustPath StatusUri => new(this, "status");
    public RobustUrl(string url)
    {
        if (!UriHelper.TryParseSs14Uri(url, out var uri))
            throw new Exception("Invalid scheme");

        Uri = uri;

        HttpUri = UriHelper.GetServerApiAddress(Uri);
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