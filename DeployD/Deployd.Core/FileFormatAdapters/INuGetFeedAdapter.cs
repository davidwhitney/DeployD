using System.Collections.Generic;
using NuGet;

namespace Deployd.Core.FileFormatAdapters
{
    public interface INuGetFeedAdapter
    {
        IList<IPackageMetadata> LoadAvailablePackages();
    }
}