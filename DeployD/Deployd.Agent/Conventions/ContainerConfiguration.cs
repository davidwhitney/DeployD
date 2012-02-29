using Deployd.Core.AgentConfiguration;
using Deployd.Core.Caching;
using Deployd.Agent.Services.Deployment;
using Deployd.Agent.Services.AgentConfiguration;
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
            Bind<IAgentSettingsManager>().To<AgentSettingsManager>();
            Bind<IAgentSettings>().ToMethod(context => GetService<IAgentSettingsManager>().LoadSettings());
            Bind<FeedLocation>().ToMethod(context => new FeedLocation { Source = GetService<IAgentSettings>().NuGetRepository });

            Bind<IRetrievePackageQuery>().To<RetrievePackageQuery>();
            Bind<IPackageRepositoryFactory>().To<PackageRepositoryFactory>();
            Bind<INuGetPackageCache>().To<NuGetPackageCache>();
            
            Bind<IAgentConfigurationDownloader>().To<AgentConfigurationDownloader>();

            Bind<IDeploymentHook>().To<PowershellScriptRunner>();
        }

        private T GetService<T>()
        {
            return (T)Kernel.GetService(typeof (T));
        }
    }
}
