using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Deployd.Agent.Services.AgentConfiguration;
using Deployd.Core.AgentConfiguration;
using NUnit.Framework;

namespace Deployd.Agent.Test.Unit.Services.AgentConfiguration
{
    [TestFixture]
    public class AgentConfigurationManagerTests
    {
        [Test]
        public void Placeholder()
        {
            var manager = new AgentConfigurationManager();

            var global = new GlobalAgentConfiguration();
            global.Environments.Add(new DeploymentEnvironment());
            global.Environments[0].Name = "Staging";
            global.Environments[0].Packages.Add("justgiving-sdk");

            manager.SaveToDisk(global, "Temp.xml");
        }
    }
}
