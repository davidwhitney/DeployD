using System;
using System.Collections.Generic;
using System.Configuration.Install;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Management;
using System.Management.Automation.Runspaces;
using System.ServiceProcess;
using Deployd.Core.AgentConfiguration;
using log4net;

namespace Deployd.Core.Installation.Hooks
{
    public class ServiceDeploymentHook : DeploymentHookBase
    {
        private string _serviceInstallationPath;

        public override bool HookValidForPackage(DeploymentContext context)
        {
            return context.Package.Tags.ToLower().Contains("service");
        }

        public ServiceDeploymentHook(IFileSystem fileSystem, IAgentSettingsManager agentSettingsManager)
            : base(agentSettingsManager, fileSystem)
        {
            _serviceInstallationPath = Path.Combine(agentSettingsManager.Settings.BaseInstallationPath, "services");
        }

        public override void BeforeDeploy(DeploymentContext context, Action<ProgressReport> reportProgress)
        {
            var logger = context.GetLoggerFor(this);
            if (!EnvironmentIsValidForPackage(context))
            {
                return;
            }

            reportProgress(new ProgressReport(context, GetType(), "Stopping service"));
            ShutdownRequiredServices(context, logger);
        }

        private void ShutdownRequiredServices(DeploymentContext context, ILog logger)
        {
            var pathToExecutable = Path.Combine(Path.Combine(_serviceInstallationPath, context.Package.Id), context.Package.Id + ".exe");
            
            var serviceName = DetermineServiceName(context, pathToExecutable, logger);
            using (var service = GetServiceByNameOrDisplayName(serviceName))
            {
                if (service == null)
                {
                    return;
                }

                // todo: recursively shut down dependent services
                if (!service.Status.Equals(ServiceControllerStatus.Running) &&
                    !service.Status.Equals(ServiceControllerStatus.StartPending))
                {
                    return;
                }

                ChangeServiceStateTo(service, ServiceControllerStatus.Stopped, service.Stop, logger);
            }
        }

        private static string DetermineServiceName(DeploymentContext context, string pathToExecutable, ILog logger)
        {
            var serviceName = context.MetaData != null
                                  ? context.MetaData.ServiceName
                                  : GetServiceNameForExecutable(context, pathToExecutable);

            if (string.IsNullOrWhiteSpace(serviceName))
            {
                serviceName = context.Package.Id;
            }
            logger.DebugFormat("Service name is '{0}'", serviceName);
            return serviceName;
        }

        public override void Deploy(DeploymentContext context, Action<ProgressReport> reportProgress)
        {
            if (!EnvironmentIsValidForPackage(context))
            {
                return;
            }

            reportProgress(new ProgressReport(context, GetType(), "Copying service files"));

            // services are installed in a '\services' subfolder
            context.TargetInstallationFolder = Path.Combine(_serviceInstallationPath, context.Package.Id);
            
            CopyAllFilesToDestination(context);
        }

        public override void AfterDeploy(DeploymentContext context, Action<ProgressReport> reportProgress)
        {


            var logger = context.GetLoggerFor(this);
            if (!EnvironmentIsValidForPackage(context))
            {
                return;
            }

            reportProgress(new ProgressReport(context, GetType(), "Starting service"));

            var pathToExecutable = Path.Combine(Path.Combine(_serviceInstallationPath, context.Package.Id),
                                                context.Package.Id + ".exe");
            var serviceName = DetermineServiceName(context, pathToExecutable, logger);

            // if no such service then install it
            try
            {
                using (var service = GetServiceByNameOrDisplayName(serviceName))
                {
                    if (service == null)
                    {
                        logger.InfoFormat("Installing service {0} from {1}", serviceName, pathToExecutable);

                        ManagedInstallerClass.InstallHelper(new[] {pathToExecutable});
                        serviceName = DetermineServiceName(context, pathToExecutable, logger);
                    }
                }

                // check that installation succeeded
                using (var service = GetServiceByNameOrDisplayName(serviceName))
                {
                    // it didn't... installutil might be presenting a credentials dialog on the terminal
                    if (service == null)
                    {
                        throw new InstallException(
                            string.Format(
                                "The executable {0} was installed, so a service named '{1}' was expected but it could not be found",
                                Path.GetFileNameWithoutExtension(pathToExecutable), serviceName));
                    }
                }

                using (var service = GetServiceByNameOrDisplayName(serviceName))
                {
                    if (!service.Status.Equals(ServiceControllerStatus.Stopped) &&
                        !service.Status.Equals(ServiceControllerStatus.StopPending))
                    {
                        return;
                    }

                    ChangeServiceStateTo(service, ServiceControllerStatus.Running, service.Start, logger);
                }
            }
            catch (Exception exception)
            {
                reportProgress(ProgressReport.Error(context, this, context.Package.Title, context.Package.Version.ToString(), context.InstallationTaskId, "Failed to install or start service " + serviceName, exception));
                throw;
            }
        }

        public override string ProgressMessage
        {
            get { return "Installing service"; }
        }

        private static ServiceController GetServiceByNameOrDisplayName(string serviceName)
        {
            return ServiceController.GetServices().SingleOrDefault(s => s.ServiceName == serviceName || s.DisplayName==serviceName);
        }

        private class ServiceInfo
        {
            public string Name { get; set; }
            public string Path { get; set; }
        }

        private static string GetServiceNameForExecutable(DeploymentContext context, string pathToExecutable)
        {
            string fileName = Path.GetFileName(pathToExecutable);
            ManagementClass mc = new ManagementClass("Win32_Service");
            List<ServiceInfo> services = new List<ServiceInfo>();

            foreach (ManagementObject mo in mc.GetInstances())
            {
                var serviceInfo = new ServiceInfo()
                                      {
                                          Name = mo.GetPropertyValue("Name").ToString(), Path = mo.GetPropertyValue("PathName").ToString()
                                      };
                if (serviceInfo.Path.Contains(" -k"))
                {
                    serviceInfo.Path = serviceInfo.Path.Substring(0,
                                                                  serviceInfo.Path.IndexOf(" -k",
                                                                                           StringComparison.
                                                                                               InvariantCultureIgnoreCase));
                }
                serviceInfo.Path = serviceInfo.Path.Trim('"');
                System.Diagnostics.Debug.WriteLine(serviceInfo.Path);
                if (serviceInfo.Path.Contains("Email"))
                {
                    System.Diagnostics.Debug.WriteLine("**" + serviceInfo.Path);
                }
                services.Add(serviceInfo);
            }

            var service = services.FirstOrDefault(s => 
                s.Path.StartsWith(pathToExecutable, StringComparison.InvariantCultureIgnoreCase));

            if (service==null)
            {
                try
                {
                    service = services.FirstOrDefault(
                        s => Path.GetFileName(s.Path)
                                 .Equals(fileName, StringComparison.InvariantCultureIgnoreCase));
                } catch
                {
                    // some services are registered with bad paths
                    // service paths are really just arbitrary strings
                }
            }

            if (service != null)
                return service.Name;

            return null;
        }

        private void ChangeServiceStateTo(ServiceController service, ServiceControllerStatus verifyMeetsThisStatus, Action switchAction, ILog logger)
        {
            logger.InfoFormat("Changing service {0} status to {1}", service.ServiceName, verifyMeetsThisStatus);
            switchAction();

            var retryCount = 10; // wait 10 retries
            while (service.Status != verifyMeetsThisStatus && --retryCount > 0)
            {
                System.Threading.Thread.Sleep(100);
                service.Refresh();
            }

            logger.InfoFormat("service is now {0}", service.Status);            
        }
    }
}
