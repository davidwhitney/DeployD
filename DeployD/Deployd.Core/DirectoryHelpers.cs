using System.IO;
using log4net;

namespace Deployd.Core
{
    public static class DirectoryHelpers
    {
        private static readonly ILog Logger = LogManager.GetLogger("DirectoryHelpers"); 
        public static void EnsureExists(string dir)
        {
            if (Directory.Exists(dir)) return;

            Logger.InfoFormat("Creating directory '{0}'.", dir);
            Directory.CreateDirectory(dir);
        
        }
    }
}