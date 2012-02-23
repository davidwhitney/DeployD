using System;
using System.Collections.Generic;
using NuGet;

namespace Deployd.Core.FileFormatAdapters
{
    public class NuGetFeedAdapter : INuGetFeedAdapter
    {
        public IList<IPackageMetadata> LoadAvailablePackages()
        {
            throw new NotImplementedException();
        } 
    }
}
