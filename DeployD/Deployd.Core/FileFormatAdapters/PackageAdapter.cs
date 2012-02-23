using NuGet;

namespace Deployd.Core.FileFormatAdapters
{
    public class PackageAdapter : IPackageAdapter
    {
        public IPackageBuilder LoadPackage(string path)
        {
            return new PackageBuilder(path);
        }
    }
}