using System;
using Deployd.Agent.Services;
using Deployd.Agent.Services.InstallationService;
using Deployd.Agent.Services.Management;
using Deployd.Agent.Services.PackageDownloading;
using Deployd.Core.AgentConfiguration;
using Deployd.Agent.Services.AgentConfiguration;
using Deployd.Core.AgentManagement;
using Deployd.Core.Hosting;
using Deployd.Core.Installation;
using Deployd.Core.Installation.Hooks;
using Deployd.Core.PackageCaching;
using Deployd.Core.PackageTransport;
using Ninject.Modules;
using NuGet;
using log4net;

namespace Deployd.Agent.Conventions
{
    public class ContainerConfiguration : NinjectModule
    {
        public override void Load()
        {
            // services
            Bind<IWindowsService>().To<AgentConfigurationService>().InSingletonScope();
            Bind<IWindowsService>().To<PackageDownloadingService>().InSingletonScope();
            Bind<IWindowsService>().To<ManagementInterfaceHost>().InSingletonScope();
            Bind<IWindowsService>().To<PackageInstallationService>().InSingletonScope();
            Bind<IWindowsService>().To<ActionExecutionService>().InSingletonScope();

            Bind<IAgentConfigurationManager>().ToMethod(context => new AgentConfigurationManager() );
            Bind<IAgentSettingsManager>().To<AgentSettingsManager>().InSingletonScope();
            Bind<IAgentSettings>().ToMethod(context => GetService<IAgentSettingsManager>().Settings);
            Bind<FeedLocation>().ToMethod(context => new FeedLocation { Source = GetService<IAgentSettings>().NuGetRepository });

            Bind<IRetrievePackageQuery>().To<RetrieveNuGetPackageQuery>();
            Bind<IPackageRepositoryFactory>().To<PackageRepositoryFactory>();
            Bind<ILocalPackageCache>().To<NuGetPackageCache>();
            
            Bind<IAgentConfigurationDownloader>().To<AgentConfigurationDownloader>();
            Bind<IDeploymentService>().To<DeploymentService>();
            Bind<IInstalledPackageArchive>().To<InstalledPackageArchive>();

            // installations management
            Bind<IInstallationManager>().To<InstallationManager>().InSingletonScope();
            Bind<RunningInstallationTaskList>().ToSelf().InSingletonScope();
            Bind<InstallationTaskQueue>().ToSelf().InSingletonScope();
            Bind<CompletedInstallationTaskList>().ToSelf().InSingletonScope();
            
            // actions management
            Bind<IAgentActionsService>().To<AgentActionsService>().InSingletonScope();
            Bind<IAgentActionsRepository>().To<AgentActionsRepository>().InSingletonScope();
            Bind<PendingActionsQueue>().ToSelf().InSingletonScope();
            Bind<CompletedActionsList>().ToSelf().InSingletonScope();
            Bind<RunningActionsList>().ToSelf().InSingletonScope();

            Bind<IDeploymentHook>().To<PowershellDeploymentHook>();
            Bind<IDeploymentHook>().To<ServiceDeploymentHook>();
            Bind<IDeploymentHook>().To<IisMsDeployDeploymentHook>();
            Bind<IDeploymentHook>().To<Iis7MsDeployDeploymentHook>();
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
