using System.Collections.Generic;

namespace Deployd.Core.AgentConfiguration
{
    public class DeploymentEnvironment
    {
        public string Name { get; set; }
        public List<string> Packages { get; set; }

        public DeploymentEnvironment()
        {
            Packages = new List<string>();
        }
    }
}