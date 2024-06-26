﻿using ContentDownloader.Data;
using ContentDownloader.Services;
using Robust.LoaderApi;

namespace ContentDownloader.Utils;

public class ContentHolder
{
    public RobustBuildInfo Info;
    public Downloader ContentDownloader;
    private HashApi? _hashApi;
    public IFileApi FileApi
    {
        get
        {
            if (_hashApi is null)
                throw new Exception("Ensure first!");
            
            return _hashApi;
        }
    }

    public AssemblyHelper AssemblyHelper;

    public bool IsEnsured => _hashApi != null;

    public ContentHolder(RobustBuildInfo info)
    {
        Info = info;
        ContentDownloader = new Downloader(Info);
    }

    public async Task EnsureItems()
    {
        ConstServices.Logger.Log("Ensure items for content");
        _hashApi = new HashApi(await ContentDownloader.EnsureItems(),ContentDownloader.Path);
        AssemblyHelper = new AssemblyHelper(_hashApi);
    }
}

public class ContentRunner
{
    public ContentHolder ContentHolder;
    public AssemblyHelper? Engine;

    public ContentRunner(RobustBuildInfo info)
    {
        ContentHolder = new ContentHolder(info);
    }

    public async Task EnsureEngine()
    {
        Engine = await ConstServices.EngineShit.EnsureEngine(ContentHolder.Info.BuildInfo.build.engine_version);
    }

    public async Task Run()
    {

        ConstServices.Logger.Log("Start Content!");
        if (Engine == null) await EnsureEngine();
        if (!ContentHolder.IsEnsured) await ContentHolder.EnsureItems();
        
        var contentApi = ContentHolder.FileApi;
        IEnumerable<ApiMount> extraMounts = new[] { new ApiMount(contentApi, "/") };
        
        var args = new MainArgs([], Engine.FileApi, new FuckRedialApi(), extraMounts);
        
        if (!Engine.TryOpenAssembly(AssemblyHelper.RobustAssemblyName, out var clientAssembly))
        {
            ConstServices.Logger.Log("Unable to locate Robust.Client.dll in engine build!");
            return;
        }

        if (!AssemblyHelper.TryGetLoader(clientAssembly, out var loader))
            return;
        
        await Task.Run(() => loader.Main(args));
    }
}

public class FuckRedialApi : IRedialApi
{
    public void Redial(Uri uri, string text = "")
    {
        
    }
}

public interface IPostInjectInit
{
  
    void PostInject();
}
