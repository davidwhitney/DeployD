using System;
using System.Collections.Generic;
using System.Configuration;

namespace Deployd.Core.AgentConfiguration
{
    public class PackageCollection : ConfigurationElementCollection
    {
        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.AddRemoveClearMap; }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new PackageSupportedInEnvironmentConfiguration();
        }

        protected override Object GetElementKey(ConfigurationElement element)
        {
            return ((PackageSupportedInEnvironmentConfiguration)element).Name;
        }
    }
}