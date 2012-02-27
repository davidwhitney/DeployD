using System.Collections.Generic;
using NuGet;

namespace Deployd.Core.Queries
{
    public interface IRetrieveAllAvailablePackageManifestsQuery
    {
        IList<IPackage> GetLatestPackage(string packageId);
    }
}