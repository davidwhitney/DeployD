using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Deployd.Agent.Core
{
    public class AgentConfiguration
    {
        public EnvironmentMode EnvironmentMode { get; set; }
        public List<string> SupportedPackages  { get; set; }
    }

    public enum EnvironmentMode
    {
        Development,
        Staging,
        Uat,
        Integration,
        Production,
        Release,
        Test,
        Sandbox,
        Dev,
        Debug
    }
}
