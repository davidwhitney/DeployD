using System;
using System.ComponentModel;
using System.ServiceProcess;
using Deployd.Core.Hosting;
using log4net;
using log4net.Config;
using ServiceInstaller = Deployd.Core.Hosting.ServiceInstaller;

namespace Deployd.Agent
{
    public class Program
    {
        private const string NAME = "Deployd.Agent";

        protected static readonly ILog Logger = LogManager.GetLogger(NAME);

        static void Main(string[] args)
        {
            XmlConfigurator.Configure();

            new WindowsServiceRunner(args,
                ()=> new IWindowsService [] { new PackageDownloadingService(), new DeploymentService() },
                installationSettings: (serviceInstaller, serviceProcessInstaller) =>
                {
                    serviceInstaller.ServiceName = NAME;
                    serviceInstaller.StartType = ServiceStartMode.Automatic;
                    serviceProcessInstaller.Account = ServiceAccount.LocalSystem;
                },
                registerContainer: () => new ContainerWrapper(),
                configureContext: x => { x.Log = s => Logger.Info(s); })
                .Host();
        }
    }

    [RunInstaller(true)]
    public class Installer : ServiceInstaller
    {
    }

    public class ContainerWrapper : IIocContainer
    {
        public T GetType<T>()
        {
            throw new NotImplementedException();
        }
    }
}
