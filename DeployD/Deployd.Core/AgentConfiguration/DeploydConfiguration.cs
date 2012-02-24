using System.Configuration;

namespace Deployd.Core.AgentConfiguration
{
    public class DeploydConfiguration : ConfigurationSection
    {
        [ConfigurationProperty("environments", IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(EnvironmentsCollection), AddItemName = "environment", ClearItemsName = "clear", RemoveItemName = "remove")]
        public EnvironmentsCollection Environments
        {
            get { return (EnvironmentsCollection)base["environments"]; }
        }
    }
}