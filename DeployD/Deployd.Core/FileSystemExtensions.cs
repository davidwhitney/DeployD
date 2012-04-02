using System;
using System.IO;
using System.IO.Abstractions;
using Deployd.Core.AgentConfiguration;
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

        public static string MapVirtualPath(this IFileSystem fs, string absolutePath)
        {
            const string virtualToken = "~\\";
            return !absolutePath.Contains(virtualToken)
                       ? absolutePath
                       : absolutePath.Replace(virtualToken, AgentSettings.AgentProgramDataPath + "\\");
        }

        public static string MapVirtualPath(this string virtualPath)
        {
            const string virtualToken = "~\\";
            return !virtualPath.Contains(virtualToken)
                       ? virtualPath
                       : virtualPath.Replace(virtualToken, AgentSettings.AgentProgramDataPath + "\\");
        }
    }
}