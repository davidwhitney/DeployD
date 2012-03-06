using System.Collections.Generic;
using System.Text;
using NuGet;

namespace Deployd.Core.Caching
{
    public interface ICurrentInstalledCache
    {
        IEnumerable<IPackage> GetCurrentInstalledPackages();
        IPackage GetCurrentInstalledVersion(string packageId);
        void SetCurrentInstalledVersion(IPackage package);
    }
}
