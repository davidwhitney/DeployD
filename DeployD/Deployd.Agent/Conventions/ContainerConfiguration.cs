using System;
using Deployd.Core.AgentConfiguration;
using Deployd.Core.Caching;
using Deployd.Agent.Services.AgentConfiguration;
using Deployd.Core.Deployment;
using Deployd.Core.Deployment.Hooks;
using Deployd.Core.Installation;
using Deployd.Core.Queries;
using Ninject.Modules;
using NuGet;
using log4net;

namespace Deployd.Agent.Conventions
{
    public class ContainerConfiguration : NinjectModule
    {
        public override void Load()
        {
            Bind<IAgentConfigurationManager>().ToMethod(context => new AgentConfigurationManager() );
            Bind<IAgentSettingsManager>().To<AgentSettingsManager>().InSingletonScope();
            Bind<IAgentSettings>().ToMethod(context => GetService<IAgentSettingsManager>().Settings);
            Bind<FeedLocation>().ToMethod(context => new FeedLocation { Source = GetService<IAgentSettings>().NuGetRepository });

            Bind<IRetrievePackageQuery>().To<RetrievePackageQuery>();
            Bind<IPackageRepositoryFactory>().To<PackageRepositoryFactory>();
            Bind<INuGetPackageCache>().To<NuGetPackageCache>();
            
            Bind<IAgentConfigurationDownloader>().To<AgentConfigurationDownloader>();

            Bind<IDeploymentService>().To<DeploymentService>();

            Bind<ICurrentInstalledCache>().To<CurrentInstalledCache>();

            Bind<IInstallationManager>().To<InstallationManager>().InSingletonScope();
            Bind<RunningInstallationTaskList>().ToSelf().InSingletonScope();
            Bind<InstallationTaskQueue>().ToSelf().InSingletonScope();
            Bind<CompletedInstallationTaskList>().ToSelf().InSingletonScope();

            Bind<IDeploymentHook>().To<PowershellDeploymentHook>();
            Bind<IDeploymentHook>().To<ServiceDeploymentHook>();
            Bind<IDeploymentHook>().To<MsDeployDeploymentHook>();
            Bind<IDeploymentHook>().To<ConfigTransformationDeploymentHook>(); // we want the config transform to run AfterDeploy() before the AppOffline hook does
            Bind<IDeploymentHook>().To<AppOfflineDeploymentHook>();
            Bind<System.IO.Abstractions.IFileSystem>().To<System.IO.Abstractions.FileSystem>();

            Bind<ILog>().ToMethod(context => LogManager.GetLogger(context.Request.Target.Name));
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
