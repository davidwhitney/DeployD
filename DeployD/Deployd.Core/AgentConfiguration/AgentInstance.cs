using System.Configuration;

namespace Deployd.Core.AgentConfiguration
{
    public class AgentInstance : ConfigurationSection, IAgentInstance
    {
        [ConfigurationProperty("machineName", IsRequired = true)]
        public string MachineName
        {
            get { return (string)this["machineName"]; }
            set { this["machineName"] = value; }
        }

        [ConfigurationProperty("ipAddress", IsRequired = true)]
        public string IpAddress
        {
            get { return (string)this["ipAddress"]; }
            set { this["ipAddress"] = value; }
        }

        [ConfigurationProperty("deploymentEnvironment", IsRequired = true)]
        public string DeploymentEnvironment
        {
            get { return (string)this["deploymentEnvironment"]; }
            set { this["deploymentEnvironment"] = value; }
        }
    }
}
