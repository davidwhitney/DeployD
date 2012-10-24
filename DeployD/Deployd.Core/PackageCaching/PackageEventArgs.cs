using System;
using NuGet;

namespace Deployd.Core.PackageCaching
{
    public class PackageEventArgs : EventArgs
    {
        public PackageEventArgs(IPackage package)
        {
            this.Package = package;
        }

        public IPackage Package { get; set; }
    }
}