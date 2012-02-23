using NuGet;

namespace Deployd.Core.NuSpecParsing
{
    public class PackageReader : IPackageReader
    {
        public IPackageBuilder LoadPackage(string path)
        {
            return new PackageBuilder(path);
        }
    }
}