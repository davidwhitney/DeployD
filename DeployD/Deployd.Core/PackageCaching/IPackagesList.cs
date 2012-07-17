using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NuGet;

namespace Deployd.Core.PackageCaching
{
    public interface IPackagesList : IList<IPackage>
    {
        IEnumerable<IPackage> GetWatched();
    }
}
