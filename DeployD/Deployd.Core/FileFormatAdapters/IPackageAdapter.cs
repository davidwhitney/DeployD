using NuGet;

namespace Deployd.Core.FileFormatAdapters
{
    public interface IPackageAdapter
    {
        IPackageBuilder LoadPackage(string path);
    }
}