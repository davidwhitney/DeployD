using System.Collections.Generic;
using NuGet;

namespace Deployd.Core.PackageCaching
{
    public interface ILocalPackageCache
    {
        IEnumerable<IPackage> AllCachedPackages();
        IList<string> AvailablePackages { get; }
        IList<string> AvailablePackageVersions(string packageId);
        void Add(IPackage package);
        void Add(IEnumerable<IPackage> allAvailablePackages);
        IPackage GetLatestVersion(string packageId);
        IPackage GetSpecificVersion(string packageId, string version);
    }
}