using System.Collections.Generic;

namespace Deployd.Core.AgentConfiguration
{
    public class DeploymentEnvironment
    {
        public string Name { get; set; }
        public List<WatchPackage> Packages { get; set; }

        public DeploymentEnvironment()
        {
            Packages = new List<WatchPackage>();
        }
    }
}