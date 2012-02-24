using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace Deployd.Core.AgentConfiguration
{
    public class DeploymentEnvironment : ConfigurationSection, IDeploymentEnvironment
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get { return this["name"] as string; }
            set { this["name"] = value; }
        }

        public IList<string> Packages
        {
            get { return (from PackageSupportedInEnvironmentConfiguration typed in PackagesRaw select typed.Name).ToList(); }
        }

        [ConfigurationProperty("packages", IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(PackageCollection), AddItemName = "package", ClearItemsName = "clear", RemoveItemName = "remove")]
        public PackageCollection PackagesRaw
        {
            get { return (PackageCollection)base["packages"]; }
        }
    }
}