using System;
using System.ComponentModel;
using System.Configuration;
using System.Configuration.Install;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;

// ReSharper disable ClassNeverInstantiated.Global
namespace Deployd.Core.Hosting
{
    /// <summary>
    /// Instantiated by the framework as part of installation.
    /// Weird looking implementation is required to work around the way the famework
    /// service installation class works (it finds a class that inherits Installer and has a RunInstaller(true) attrib
    /// creates an instance of it and executes it. To enable configuration, we need to punch an ScriptName shaped hole in the side
    /// that can be invoked by the zero param ctor)
    /// </summary>
    [RunInstaller(true)]
    public class ServiceInstaller : Installer
    {
        private static Action<ServiceInstaller> _configureAction = configuration => { };
        
        public static void PerformAnyRequestedInstallations(string[] args, ApplicationContext context, string assemblyLocation = null)
        {
            if(assemblyLocation == null)
            {
                assemblyLocation = Assembly.GetEntryAssembly().Location;
            }

            var parameter = string.Concat(args);
            _configureAction = x => x.ConfigureServiceInstall(context);

            switch (parameter)
            {
                case "-install":
                case "/install":
                case "-i":
                case "/i":
                    InstallAssemblyAsService(assemblyLocation);
                    break;
                case "-uninstall":
                case "/uninstall":
                case "-u":
                case "/u":
                    UninstallService(assemblyLocation);
                    break;
                case "-tryinstall":
                case "/tryinstall":
                case "-ti":
                case "/ti":
                    TryInstallAsService(assemblyLocation);
                    break;
            }
        }

        private static void TryInstallAsService(string assemblyLocation)
        {
            try
            {
                InstallAssemblyAsService(assemblyLocation);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Errored while trying to install service. If the service is already installed, this is fine. Error was: " + ex.Message);
                Environment.Exit(0);
            }
        }

        private static void InstallAssemblyAsService(string assemblyLocation)
        {
            ManagedInstallerClass.InstallHelper(new[] { assemblyLocation });
            Console.WriteLine("Installed");
            Environment.Exit(0);
        }

        private static void UninstallService(string assemblyLocation)
        {
            ManagedInstallerClass.InstallHelper(new[] { "/u", assemblyLocation});
            Console.WriteLine("Uninstalled");
            Environment.Exit(0);
        }


        // Below = class instantiated by the framework installation classes

        private readonly ServiceProcessInstaller _serviceProcessInstaller = new ServiceProcessInstaller();
        private readonly System.ServiceProcess.ServiceInstaller _serviceInstaller = new System.ServiceProcess.ServiceInstaller();

        public ServiceInstaller()
        {
            _configureAction(this);
        }

        private void ConfigureServiceInstall(ApplicationContext context)
        {
            _serviceInstaller.ServiceName = Guid.NewGuid().ToString();
            _serviceInstaller.StartType = ServiceStartMode.Manual;
            _serviceProcessInstaller.Account = ServiceAccount.LocalSystem;

            context.ConfigureInstall(_serviceInstaller, _serviceProcessInstaller);

            Installers.AddRange(new Installer[] { _serviceProcessInstaller, _serviceInstaller });
        }

        public override void Install(System.Collections.IDictionary stateSaver)
        {
            base.Install(stateSaver);

            string nugetRepositoryUrl = Context.Parameters["NugetRepository"];
            string environment = Context.Parameters["Environment"];

            string exePath = string.Format("{0}Deployd.Agent.exe", Context.Parameters["targetdir"]);
            var config = ConfigurationManager.OpenExeConfiguration(exePath);
            config.AppSettings.Settings["NuGetRepository"].Value = nugetRepositoryUrl;
            config.AppSettings.Settings["DeploymentEnvironment"].Value = environment;
            config.Save();
        }

        public static bool IsServiceInstalled(string serviceName)
        {
            var services = ServiceController.GetServices();
            return services.Any(service => service.ServiceName == serviceName);
        }
    }
}
// ReSharper restore ClassNeverInstantiated.Global