using System.Collections.Generic;
using NuGet;

namespace Deployd.Core.Caching
{
    public interface INuGetPackageCache
    {
        void Cache(IPackage package);
        void Cache(IList<IPackage> allAvailablePackages);
    }
}