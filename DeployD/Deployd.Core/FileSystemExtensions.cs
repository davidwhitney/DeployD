using System.IO;
using System.IO.Abstractions;
using log4net;

namespace Deployd.Core
{
    public static class FileSystemExtensions
    {
        private static readonly ILog Logger = LogManager.GetLogger("IFileSystemExtensions"); 
        
        public static void EnsureDirectoryExists(this IFileSystem fs, string dir)
        {
            if (fs.Directory.Exists(dir)) return;

            Logger.InfoFormat("Creating directory '{0}'.", dir);
            fs.Directory.CreateDirectory(dir);
        
        }

        public static string MapVirtualPath(this IFileSystem fs, string path)
        {
            return path.Replace("~\\", fs.Directory.GetCurrentDirectory() + "\\");
        }
    }
}