using ContentDownloader.Utils;
using Robust.LoaderApi;

namespace ContentDownloader.Data;

public class FunnyApi : IFileApi
{
    public IReadOnlyList<IFileApi> Apis;
    public List<string> Files;


    public FunnyApi(params IFileApi[] apis)
    {
        Apis = apis;
        Files = new List<string>();
        foreach (var api in apis)
        {
            foreach (var file in api.AllFiles)
            {
                if(!Files.Contains(file))
                    Files.Add(file);
            }
        }
    }

    public bool TryOpen(string path, out Stream? stream)
    {
        foreach (var api in Apis)
        {
            if (api.TryOpen(path, out stream)) return true;
        }

        stream = null;
        return false;
    }

    public IEnumerable<string> AllFiles => Files;
}

public class FileApi : IFileApi
{
    public string RootPath;
    public string MountPath;
    
    public FileApi(string rootPath, string mountPath)
    {
        RootPath = rootPath;
        MountPath = mountPath;
    }
    
    public bool TryOpen(string path, out Stream? stream)
    {
        if (File.Exists(RootPath + MountPath + path))
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
            return Directory.EnumerateFiles(RootPath, "*.*", SearchOption.AllDirectories).Select(s => s.Replace(RootPath,MountPath));
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