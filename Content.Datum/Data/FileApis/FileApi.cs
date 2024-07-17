﻿using Content.Datum.Data.FileApis.Interfaces;

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
            stream = File.OpenRead(RootPath + path);
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

    public bool Has(string path)
    {
        var currPath = Path.Join(RootPath, path);
        return File.Exists(currPath);
    }

    public IEnumerable<string> AllFiles => Directory.EnumerateFiles(RootPath, "*.*", SearchOption.AllDirectories);
}