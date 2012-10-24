using System;
using Deployd.Agent.Services.HubCommunication;
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
using Deployd.Core.Notifications;
using Deployd.Core.PackageCaching;
using Deployd.Core.PackageTransport;
using Deployd.Core.Remoting;
using Ninject;
using Ninject.Modules;
using NuGet;
using log4net;

namespace Deployd.Agent.Conventions
{
    public class ContainerConfiguration : NinjectModule
    {
        public override void Load()
        {
            // configuration
            Bind<IAgentWatchListManager>().To<AgentWatchListManager>();
            Bind<IAgentWatchList>().ToMethod(ctx => ctx.Kernel.Get<IAgentWatchListManager>().Build());
            Bind<IPackageGroupConfiguration>().ToMethod(ctx => PackageGroupConfigurationFactory.Build());
            Bind<IConfigurationDefaults>().To<ConfigurationDefaults>();

            // services
            Bind<IWindowsService>().To<NotificationService>().InSingletonScope(); // make sure this is first so it is ready to send messages from other services
            Bind<IWindowsService>().To<AgentConfigurationService>().InSingletonScope();
            Bind<IWindowsService>().To<PackageDownloadingService>().InSingletonScope();
            Bind<IWindowsService>().To<ManagementInterfaceHost>().InSingletonScope();
            Bind<IWindowsService>().To<PackageInstallationService>().InSingletonScope();
            Bind<IWindowsService>().To<ActionExecutionService>().InSingletonScope();
            Bind<IWindowsService>().To<HubCommunicationService>().InSingletonScope();
            Bind<IWindowsService>().To<HubCommunicationsQueueService>().InSingletonScope();
            
            // hub comms
            Bind<HubCommunicationService>().ToSelf().InSingletonScope();
            Bind<HubCommunicationsQueue>().ToSelf().InSingletonScope();
            Bind<IHubCommunicator>().To<HubCommunicator>();

            Bind<IAgentConfigurationManager>().To<AgentConfigurationManager>().InSingletonScope();
            Bind<IAgentSettingsManager>().To<AgentSettingsManager>().InSingletonScope();
            Bind<IAgentSettings>().ToMethod(context => GetService<IAgentSettingsManager>().Settings);
            Bind<FeedLocation>().ToMethod(context => new FeedLocation { Source = GetService<IAgentSettings>().NuGetRepository });

            Bind<IRetrievePackageQuery>().To<RetrieveNuGetPackageQuery>();
            Bind<IPackageRepositoryFactory>().To<PackageRepositoryFactory>();
            Bind<ILocalPackageCache>().To<NuGetPackageCache>();
            
            Bind<IAgentConfigurationDownloader>().To<AgentConfigurationDownloader>();
            Bind<IDeploymentService>().To<DeploymentService>();
            Bind<IInstalledPackageArchive>().To<InstalledPackageArchive>();
            Bind<IPackagesList>().To<AllPackagesList>().InSingletonScope();
            Bind<ICurrentlyDownloadingList>().To<CurrentlyDownloadingList>().InSingletonScope();

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

            // deployment hooks
            Bind<IDeploymentHook>().To<PowershellDeploymentHook>();
            Bind<IDeploymentHook>().To<ConfigTransformationDeploymentHook>(); // we want the config transform to run AfterDeploy() before the AppOffline hook does
            Bind<IDeploymentHook>().To<ServiceDeploymentHook>();
            Bind<IDeploymentHook>().To<IisMsDeployDeploymentHook>();
            Bind<IDeploymentHook>().To<Iis7MsDeployDeploymentHook>();
            Bind<IDeploymentHook>().To<AppOfflineDeploymentHook>();
            Bind<IDeploymentHook>().To<CustomActionsExtractionDeploymentHook>();
            Bind<System.IO.Abstractions.IFileSystem>().To<System.IO.Abstractions.FileSystem>();

            // notifiers
            Bind<INotificationService>().To<NotificationService>().InSingletonScope();
            Bind<INotifier>().To<JabberNotifier>().InSingletonScope();
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
