using Content.Datum.Data.FileApis.Interfaces;

namespace Content.Datum.Data.FileApis;

public class FileApi : IReadWriteFileApi
{
    public string RootPath;
    
    public FileApi(string rootPath)
    {
        RootPath = rootPath;
    }
    
    public bool TryOpen(string path, out Stream? stream)
    {
        if (File.Exists(Path.Join(RootPath, path)))
        {
            stream = File.Open(RootPath + path,FileMode.Open);
            return true;
        }

        stream = null;
        return false;
    }
    
    public bool Save(string path, Stream input)
    {
        var currPath = Path.Join(RootPath, path);
        
        var dirInfo = new DirectoryInfo(Path.GetDirectoryName(currPath));
        if (!dirInfo.Exists)
        {
            dirInfo.Create();
        }

        using var stream = File.OpenWrite(currPath);
        input.CopyTo(stream);
        stream.Close();
        return true;
    }

    public IEnumerable<string> AllFiles => Directory.EnumerateFiles(RootPath, "*.*", SearchOption.AllDirectories);
}