using Content.Datum.Data.FileApis.Interfaces;
using Robust.LoaderApi;

namespace Content.Datum.Data.FileApis;

public class MountApi : IReadWriteFileApi
{
    private Dictionary<string, IFileApi> FileApis = new();

    public string Splitter = "\\";

    public void Mount(string path, IFileApi fileApi)
    {
        FileApis.Add(path, fileApi);
    }

    public void UnMount(string path)
    {
        FileApis.Remove(path);
    }
    
    public bool TryOpen(string path, out Stream? stream)
    {
        foreach (var (key,value) in FileApis)
        {
            if (key.StartsWith(path))
            {
                return value.TryOpen(path.Substring(key.Length), out stream);
            }
        }

        stream = null;
        return false;
    }

    public IEnumerable<string> AllFiles
    {
        get
        {
            foreach (var (key,value) in FileApis)
            {
                foreach (var file in value.AllFiles)
                {
                    yield return Path.Join(key,file);
                }
            }
        }
    }

    public bool Save(string path, Stream input)
    {
        foreach (var (key,value) in FileApis)
        {
            if (key.StartsWith(path) && value is IWriteFileApi writeFileApi)
            {
                return writeFileApi.Save(path.Substring(key.Length), input);
            }
        }

        return false;

    }

    public bool Has(string path)
    {
        foreach (var (key,value) in FileApis)
        {
            if (key.StartsWith(path) && value is IReadWriteFileApi readWriteFileApi)
            {
                return readWriteFileApi.Has(path.Substring(key.Length));
            }
        }

        return false;
    }
}