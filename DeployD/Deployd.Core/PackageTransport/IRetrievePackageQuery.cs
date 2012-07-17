using System;
using NuGet;

namespace Deployd.Core.PackageTransport
{
    public interface IRetrievePackageQuery
    {
        IPackage GetLatestPackage(string packageId);
        IPackage GetSpecificPackage(string packageId, string version);
    }
}
