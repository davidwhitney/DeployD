using System.IO;
using NuGet;
using log4net;

namespace Deployd.Agent.Services.Deployment
{
    public class PackageExtractor
    {
        private ILog Logger = LogManager.GetLogger("PackageExtractor");
        public void Extract(string packagePath, string destinationPath)
        {
            Extract(new ZipPackage(packagePath), destinationPath);
        }

        public void Extract(IPackage package, string destinationPath)
        {
            if (!Directory.Exists(destinationPath))
            {
                Directory.CreateDirectory(destinationPath);
            }

            var files = package.GetFiles();
            foreach(var file in files)
            {
                string fileOutputPath = Path.Combine(destinationPath, file.Path);
                Logger.DebugFormat("Writing file {0} to {1}...", file.Path, fileOutputPath);
                string directoryPath = Path.GetDirectoryName(fileOutputPath);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                File.WriteAllBytes(fileOutputPath, file.GetStream().ReadAllBytes());
            }
        }
    }
}