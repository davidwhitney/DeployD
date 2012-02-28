using Deployd.Core.Caching;
using Deployd.Agent.Services.Deployment;
using System.IO;
using Deployd.Agent.Services.AgentConfiguration;
using Deployd.Core.Caching;
using Deployd.Core.Queries;
using Ninject.Modules;
using NuGet;

namespace Deployd.Agent.Conventions
{
    public class ContainerConfiguration : NinjectModule
    {
        public override void Load()
        {

            Bind<IAgentConfigurationManager>().ToMethod(context => new AgentConfigurationManager() );

            Bind<IRetrieveAllAvailablePackageManifestsQuery>().To<RetrieveAllAvailablePackageManifestsQuery>();
            Bind<IPackageRepositoryFactory>().To<PackageRepositoryFactory>();
            Bind<INuGetPackageCache>().To<NuGetPackageCache>();
            

            Bind<FeedLocation>().ToMethod(context => new FeedLocation
                                                         {
                                                             Source = Directory.GetCurrentDirectory() + @"\DebugPackageSource"
                                                         });

            Bind<DeploymentService>().To<DeploymentService>();

            Bind<IDeploymentHook>().To<DefaultDeploymentHook>();
            Bind<IDeploymentHook>().To<PowershellDeploymentHook>();
        }
    }
}
