using System.Collections.Generic;
using NuGet;

namespace Deployd.Core.Queries
{
    public interface IRetrievePackageQuery
    {
        IList<IPackage> GetLatestPackage(string packageId);
    }
}