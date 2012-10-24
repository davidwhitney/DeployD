using System;
using System.Linq;
using Deployd.Core.Installation;

namespace Deployd.Core
{
    public class PackageViewModel
    {
        public string packageId { get; set; }
        public string[] availableVersions { get; set; }
        public bool installed { get; set; }
        public string installedVersion { get; set; }
        public InstallTaskViewModel currentTask { get; set; }
        public InstallationResult installationResult { get; set; }
        public string latestVersion
        {
            get { return availableVersions.Max(v => Version.Parse(v)).ToString(); }
            set { }
        }
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