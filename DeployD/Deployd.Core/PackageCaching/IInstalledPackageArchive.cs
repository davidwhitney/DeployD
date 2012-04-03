using System.Collections.Generic;
using NuGet;

namespace Deployd.Core.PackageCaching
{
    public interface IInstalledPackageArchive
    {
        IEnumerable<IPackage> GetCurrentInstalledPackages();
        IPackage GetCurrentInstalledVersion(string packageId);
        void SetCurrentInstalledVersion(IPackage package);
    }
}
