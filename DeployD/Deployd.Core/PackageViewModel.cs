using System;
using System.Linq;

namespace Deployd.Core
{
    public class PackageViewModel
    {
        public string packageId { get; set; }
        public string[] availableVersions { get; set; }
        public bool installed { get; set; }
        public string installedVersion { get; set; }
        public InstallTaskViewModel currentTask { get; set; }
        public bool outOfDate
        {
            get
            {
                if (availableVersions != null && availableVersions.Length > 0)
                {
                    if (installedVersion != null)
                    {
                        return Version.Parse(installedVersion) < availableVersions.Max(v => Version.Parse(v));
                    } 
                }
                return false;
            }
            set { }
        }
    }
}