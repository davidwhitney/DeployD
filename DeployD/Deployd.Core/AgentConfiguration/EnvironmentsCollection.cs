using System;
using System.Configuration;

namespace Deployd.Core.AgentConfiguration
{
    public class EnvironmentsCollection : ConfigurationElementCollection
    {
        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.AddRemoveClearMap; }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new DeploymentEnvironment();
        }

        protected override Object GetElementKey(ConfigurationElement element)
        {
            return ((DeploymentEnvironment)element).Name;
        }
    }
}