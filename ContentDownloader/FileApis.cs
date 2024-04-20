using Robust.LoaderApi;

namespace ContentDownloader;

public class FileApi : IFileApi
{
    public string RootPath;
    public FileApi(string rootPath)
    {
        RootPath = rootPath;
    }
    
    public bool TryOpen(string path, out Stream? stream)
    {
        if (File.Exists(RootPath + path))
        {
            stream = File.Open(RootPath + path,FileMode.Open);
            return true;
        }

        stream = null;
        return false;
    }

    public IEnumerable<string> AllFiles
    {
        get
        {
            return Directory.EnumerateFiles(RootPath, "*.*", SearchOption.AllDirectories);
        }
    }
}

public class HashApi : IFileApi
{
    public string RootPath;
    public Dictionary<string, RobustManifestItem> Manifest;

    public HashApi(List<RobustManifestItem> manifest, string rootPath)
    {
        RootPath = rootPath;
        Manifest = new Dictionary<string, RobustManifestItem>();
        foreach (var item in manifest)
        {
            Manifest.TryAdd(item.Path, item);
        }
    }

    public bool TryOpen(string path, out Stream? stream)
    {
        if (path[0] == '/')
        {
            path = path.Substring(1);
        }
        if (!Manifest.TryGetValue(path, out var a) && !File.Exists(RootPath + a.Hash))
        {
            stream = null;
            return false;
        }

        stream = File.OpenRead(RootPath + a.Hash);
        return true;

    }

    public IEnumerable<string> AllFiles => Manifest.Keys;
}