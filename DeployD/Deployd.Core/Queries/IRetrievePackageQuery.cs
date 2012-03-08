using NuGet;

namespace Deployd.Core.Queries
{
    public interface IRetrievePackageQuery
    {
        IPackage GetLatestPackage(string packageId);
    }
}