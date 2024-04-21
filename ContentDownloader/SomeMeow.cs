using System.Reflection;
using ContentDownloader;
using ContentDownloader.Utils;

namespace Robust.Client
{
    public class SmeshnoeIoC
    {
        public void Register()
        {
            IoCManager.Register<SomeSittyFarm>();
        }
    }

    public class IoCManager
    {
        public static void Register<T>()
        {
           //Assembly.GetAssembly()
        }
    }

    public class SomeSittyFarm : IPostInjectInit
    {
        public void PostInject()
        {
            
        }
    }
}