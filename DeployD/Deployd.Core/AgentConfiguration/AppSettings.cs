using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Deployd.Core.AgentConfiguration
{
    public class AppSettings : Dictionary<string,string>, IAgentSettings
    {
        public int PackageSyncIntervalMs
        {
            get { return Int32.Parse(this["PackageSyncIntervalMs"]); }
        }

        public int ConfigurationSyncIntervalMs
        {
            get { return Int32.Parse(this["PackageSyncIntervalMs"]); }
        }

        public string DeploymentEnvironment
        {
            get { return this["PackageSyncIntervalMs"]; }
        }

        public string InstallationDirectory
        {
            get { return this["PackageSyncIntervalMs"]; }
        }

        public string NuGetRepository
        {
            get { return this["PackageSyncIntervalMs"]; }
        }

        public string UnpackingLocation
        {
            get { return this["PackageSyncIntervalMs"]; }
        }

        public AppSettings(NameValueCollection inner)
        {
            foreach (var key in inner.AllKeys)
            {
                Add(key, inner[key]);
            }
        }
    }
}