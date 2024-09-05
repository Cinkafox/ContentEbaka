﻿using System.IO.Compression;
using System.Runtime.InteropServices;
using Content.Datum.Data.FileApis;
using Content.Datum.Data.FileApis.Interfaces;
using Content.Datum.Utils;
using Robust.LoaderApi;

namespace Content.Datum.Services;

public class FileService
{
    public static string RootPath = Path.Join(Environment.GetFolderPath(
        Environment.SpecialFolder.ApplicationData), "./Datum/");

    private readonly DebugService _debugService;
    
    public readonly IReadWriteFileApi ContentFileApi;
    public readonly IReadWriteFileApi EngineFileApi;
    public readonly IReadWriteFileApi ManifestFileApi;
    
    private IFileApi? _hashApi;

    public FileService(DebugService debugService)
    {
        _debugService = debugService;
        ContentFileApi = CreateFileApi("content/");
        EngineFileApi = CreateFileApi("engine/");
        ManifestFileApi = CreateFileApi("manifest/");
    }

    public List<RobustManifestItem> ManifestItems
    {
        set => HashApi = new HashApi(value, ContentFileApi);
    }

    public IFileApi HashApi
    {
        get
        {
            if (_hashApi is null) throw new Exception("Hash API is not initialized!");
            return _hashApi;
        }
        set => _hashApi = value;
    }

    public IReadWriteFileApi CreateFileApi(string path)
    {
        return new FileApi(Path.Join(RootPath, path));
    }

    public ZipFileApi OpenZip(string path, IFileApi fileApi)
    {
        if (!fileApi.TryOpen(path, out var zipStream))
            return null;

        var zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Read);

        var prefix = "";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) prefix = "Space Station 14.app/Contents/Resources/";
        return new ZipFileApi(zipArchive, prefix);
    }
}