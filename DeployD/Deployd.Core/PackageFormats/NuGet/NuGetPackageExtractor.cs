using System.IO;
using NuGet;
using ILogger = Ninject.Extensions.Logging.ILogger;

namespace Deployd.Core.PackageFormats.NuGet
{
    public class NuGetPackageExtractor
    {
        private readonly ILogger _logger;

        public NuGetPackageExtractor(ILogger logger)
        {
            _logger = logger;
        }

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
                _logger.Debug(string.Format("Writing file {0} to {1}...", file.Path, fileOutputPath));
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