using System.Collections.Generic;

namespace Deployd.Core.AgentConfiguration
{
    public interface IDeploymentEnvironment
    {
        string Name { get; set; }
        IList<string> Packages { get; }
    }
}