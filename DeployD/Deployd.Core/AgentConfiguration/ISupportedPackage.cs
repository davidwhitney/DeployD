using System.Configuration;

namespace Deployd.Core.AgentConfiguration
{
    public interface ISupportedPackage
    {
        [ConfigurationProperty("name", IsRequired = true)]
        string Name { get; set; }
    }
}