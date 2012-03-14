using System;
using Deployd.Agent.Conventions;
using Deployd.Agent.Services.AgentConfiguration;
using Deployd.Core.AgentConfiguration;
using Deployd.Core.Caching;
using Deployd.Core.Deployment;
using Deployd.Core.Deployment.Hooks;
using Deployd.Core.Installation;
using Deployd.Core.Queries;
using NUnit.Framework;
using Ninject;
using Ninject.Modules;
using NuGet;

namespace Deployd.Agent.Test.Unit.Conventions
{
    [TestFixture]
    public class ContainerConfigurationTests
    {
        private ContainerConfiguration _containerConfig;

        [SetUp]
        public void SetUp()
        {
            _containerConfig = new ContainerConfiguration();
            new StandardKernel(new INinjectModule[] {_containerConfig });
        }

        [TestCase(typeof(IAgentConfigurationManager))]
        [TestCase(typeof(IAgentSettingsManager))]
        [TestCase(typeof(IAgentSettings))]
        [TestCase(typeof(FeedLocation))]
        [TestCase(typeof(IRetrievePackageQuery))]
        [TestCase(typeof(IPackageRepositoryFactory))]
        [TestCase(typeof(INuGetPackageCache))]
        [TestCase(typeof(IAgentConfigurationDownloader))]
        [TestCase(typeof(IDeploymentHook))]
        [TestCase(typeof(IDeploymentService))]
        [TestCase(typeof(IInstallationManager))]
        [TestCase(typeof(System.IO.Abstractions.IFileSystem))]
        public void GetType_CanInstantiateBoundDependency_DoesNotThrow(Type type)
        {
            var item = _containerConfig.GetService(type);

            Assert.That(item, Is.Not.Null);
        }
    }
}
