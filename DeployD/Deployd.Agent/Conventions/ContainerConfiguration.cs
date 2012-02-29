using System;
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

            Bind<IDeploymentService>().To<DeploymentService>();

            Bind<IDeploymentHook>().To<PowershellScriptRunner>();
            Bind<System.IO.Abstractions.IFileSystem>().To<System.IO.Abstractions.FileSystem>();
        }

        public T GetService<T>()
        {
            return (T) GetService(typeof (T));
        }

        public object GetService(Type type)
        {
            return Kernel.GetService(type);
        }
    }
}
