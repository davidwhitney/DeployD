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
        protected static readonly ILog Logger = LogManager.GetLogger("Deployd.Agent");

        static void Main(string[] args)
        {
            XmlConfigurator.Configure();

            new WindowsServiceRunner(args,
                ()=> new IWindowsService [] { new DeploymentService() },
                installationSettings: (serviceInstaller, serviceProcessInstaller) =>
                {
                    serviceInstaller.ServiceName = "GG.PaymentProcessing.Agent";
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
