using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Deployd.Core.AgentConfiguration
{
    public class AppSettings : Dictionary<string,string>, IAgentSettings
    {
        private XMPPSettings _xmppSettings=null;

        public int PackageSyncIntervalMs
        {
            get { return Int32.Parse(this["PackageSyncIntervalMs"]); }
        }

        public int ConfigurationSyncIntervalMs
        {
            get { return Int32.Parse(this["ConfigurationSyncIntervalMs"]); }
        }

        public string DeploymentEnvironment
        {
            get { return this["DeploymentEnvironment"]; }
        }

        public string InstallationDirectory
        {
            get { return this["InstallationDirectory"]; }
        }

        public string NuGetRepository
        {
            get { return this["NuGetRepository"]; }
        }

        public string[] Tags
        {
            get { return this["Tags"].ToLower().Split(' ', ',', ';'); }
        }

        public string LatestDirectory
        {
            get { return this["LatestDirectory"]; }
        }

        public string CacheDirectory
        {
            get { return this["CacheDirectory"]; }
        }

        public string UnpackingLocation
        {
            get { return this["UnpackingLocation"]; }
        }

        public string BaseInstallationPath
        {
            get { return this["BaseInstallationPath"]; }
        }

        public string MsDeployServiceUrl
        {
            get { return this["MsDeployServiceUrl"]; }
        }

        public string LogsDirectory
        {
            get { return this["LogsDirectory"]; }
        }

        public string HubAddress
        {
            get { return this["Hub.Address"]; }
        }

        public bool EnableConfigurationSync
        {
            get { return bool.Parse(this["EnableConfigurationSync"]); }
        }

        public string NotificationRecipients
        {
            get { return this["Notifications.Recipients"]; }
        }

        public IXMPPSettings XMPPSettings
        {
            get
            {
                if (_xmppSettings==null)
                {
                    _xmppSettings = new XMPPSettings()
                    {
                        Host = this["Notifications.XMPP.Host"],
                        Username = this["Notifications.XMPP.Username"],
                        Password = this["Notifications.XMPP.Password"],
                        Port = int.Parse(this["Notifications.XMPP.Port"]),
                        Enabled = bool.Parse(this["Notifications.XMPP.Enabled"]),
                    };
                }
                return _xmppSettings;
            }
            set {}
        }

        public int MaxConcurrentInstallations { get { return Math.Max(1, int.Parse(this["MaxConcurrentInstallations"])); } }
    }
}