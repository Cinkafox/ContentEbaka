using System.IO.Compression;
using System.Runtime.InteropServices;
using Content.Datum.Data.FileApis;
using Content.Datum.Data.FileApis.Interfaces;
using Content.Datum.Utils;
using Robust.LoaderApi;

namespace Content.Datum.Services;

public class FileService
{
    private readonly DebugService _debugService;
    
    public readonly MountApi FileApi = new MountApi();
    public IFileApi ReadOnlyFileApi => FileApi;
    public IWriteFileApi WriteOnlyFileApi => FileApi;

    public string RootPath = "./datum/";

    public FileService(DebugService debugService)
    {
        _debugService = debugService;
    }

    public void Mount(string path,IReadWriteFileApi fileApi)
    {
        FileApi.Mount(path, fileApi);
    }

    public IReadWriteFileApi CreateAndMount(string path)
    {
        var fileApi = new FileApi(Path.Join(RootPath, path));
        Mount(path,fileApi);
        return fileApi;
    }
    
    public ZipFileApi OpenZip(string path)
    {
        if (!FileApi.TryOpen(path, out var zipStream))
            return null;
        
        var zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Read);
        
        var prefix = "";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            prefix = "Space Station 14.app/Contents/Resources/";
        }
        return new ZipFileApi(zipArchive, prefix);
    }

    public HashApi GetHashApi(List<RobustManifestItem> items, IFileApi fileApi)
    {
        return new HashApi(items, fileApi);
    }
}