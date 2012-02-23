using NuGet;

namespace Deployd.Core.NuSpecParsing
{
    public interface IPackageReader
    {
        IPackageBuilder LoadPackage(string path);
    }
}