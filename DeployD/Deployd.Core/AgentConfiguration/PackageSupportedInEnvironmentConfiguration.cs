using System.Configuration;

namespace Deployd.Core.AgentConfiguration
{
    public class PackageSupportedInEnvironmentConfiguration : ConfigurationSection, ISupportedPackage
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get { return this["name"] as string; }
            set { this["name"] = value; }
        }
    }
}