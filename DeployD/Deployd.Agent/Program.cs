using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using Deployd.Agent.Conventions;
using Deployd.Agent.Services.AgentConfiguration;
using Deployd.Agent.Services.InstallationService;
using Deployd.Agent.Services.Management;
using Deployd.Agent.Services.PackageDownloading;
using Deployd.Core;
using Deployd.Core.AgentConfiguration;
using Deployd.Core.Hosting;
using Ninject;
using Ninject.Modules;
using log4net;
using log4net.Appender;
using log4net.Config;
using ServiceInstaller = Deployd.Core.Hosting.ServiceInstaller;

namespace Deployd.Agent
{
    public class Program
    {
        private const string NAME = "Deployd.Agent";

        protected static readonly ILog Logger = LogManager.GetLogger(NAME);
        private static IKernel _kernel;
        private static ContainerWrapper _containerWrapper;

        static void Main(string[] args)
        {
            XmlConfigurator.Configure();

            _kernel = new StandardKernel(new INinjectModule[]{new ContainerConfiguration()});
            _containerWrapper = new ContainerWrapper(_kernel);

            var agentSettings = _containerWrapper.GetType<IAgentSettings>();

            SetLogAppenderPaths(agentSettings);

            new WindowsServiceRunner(args,
                                        () => new IWindowsService[]
                                                {
                                                    _kernel.Get<AgentConfigurationService>(),
                                                    _kernel.Get<PackageDownloadingService>(),
                                                    _kernel.Get<ManagementInterfaceHost>(),
                                                    _kernel.Get<PackageInstallationService>()
                                                },
                                        installationSettings: (serviceInstaller, serviceProcessInstaller) =>
                                                                {
                                                                    serviceInstaller.ServiceName = NAME;
                                                                    serviceInstaller.StartType =
                                                                        ServiceStartMode.Manual;
                                                                    serviceProcessInstaller.Account =
                                                                        ServiceAccount.User;
                                                                },
                                        registerContainer: () => _containerWrapper,
                                        configureContext: x => { x.Log = s => Logger.Info(s); })
                .Host();

            
        }

        private static void SetLogAppenderPaths(IAgentSettings agentSettings)
        {
            var appenders = Logger.Logger.Repository.GetAppenders().Where(a=>a is FileAppender);
            foreach (FileAppender appender in appenders)
            {
                string fileName = Path.GetFileName(appender.File);
                appender.File = Path.Combine(agentSettings.LogsDirectory.MapVirtualPath(), fileName);
                appender.ActivateOptions();
            }
        }
    }

    [RunInstaller(true)]
    public class Installer : ServiceInstaller
    {
    }

    public class ContainerWrapper : IIocContainer
    {
        private readonly IKernel _kernel;

        public ContainerWrapper(IKernel kernel)
        {
            _kernel = kernel;
        }

        public T GetType<T>()
        {
            return _kernel.Get<T>();
        }
    }
}
