using System;
using System.Configuration.Install;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
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

        public ServiceDeploymentHook(IFileSystem fileSystem, IAgentSettings agentSettings)
            : base(agentSettings, fileSystem)
        {
            _serviceInstallationPath = Path.Combine(agentSettings.BaseInstallationPath, "services");
        }

        public override void BeforeDeploy(DeploymentContext context)
        {
            var logger = context.GetLoggerFor(this);
            if (!EnvironmentIsValidForPackage(context))
            {
                return;
            }

            ShutdownRequiredServices(context, logger);
        }

        private void ShutdownRequiredServices(DeploymentContext context, ILog logger)
        {
            var pathToExecutable = Path.Combine(context.TargetInstallationFolder, context.Package.Id + ".exe");
            var serviceName = GetServiceNameForExecutable(context, pathToExecutable);
            if (string.IsNullOrWhiteSpace(serviceName))
            {
                serviceName = context.Package.Id;
            }
            using (var service = ServiceController.GetServices().SingleOrDefault(s => s.ServiceName == serviceName))
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

        public override void Deploy(DeploymentContext context)
        {
            if (!EnvironmentIsValidForPackage(context))
            {
                return;
            }

            // services are installed in a '\services' subfolder
            context.TargetInstallationFolder = Path.Combine(_serviceInstallationPath, context.Package.Id);
            
            CopyAllFilesToDestination(context);
        }

        public override void AfterDeploy(DeploymentContext context)
        {
            var logger = context.GetLoggerFor(this);
            if (!EnvironmentIsValidForPackage(context))
            {
                return;
            }

            var pathToExecutable = Path.Combine(context.TargetInstallationFolder, context.Package.Id + ".exe");
            var serviceName = GetServiceNameForExecutable(context, pathToExecutable);
            // if no such service then install it
            using (var service = ServiceController.GetServices().SingleOrDefault(s => s.ServiceName == serviceName))
            {
                if (service == null)
                {
                    logger.InfoFormat("Installing service {0} from {1}", context.Package.Title, pathToExecutable);

                    ManagedInstallerClass.InstallHelper(new[] {pathToExecutable});
                }
            }

            // check that installation succeeded
            using (var service = ServiceController.GetServices().SingleOrDefault(s => s.ServiceName == serviceName))
            {
                // it didn't... installutil might be presenting a credentials dialog on the terminal
                if (service == null)
                {
                    throw new InstallException(string.Format("The executable {0} was installed, so a service named '{1}' was expected but it could not be found", Path.GetFileNameWithoutExtension(pathToExecutable), serviceName));
                }
            }

            using (var service = ServiceController.GetServices().SingleOrDefault(s => s.ServiceName == serviceName))
            {
                // todo: recursively shut down dependent services
                if (!service.Status.Equals(ServiceControllerStatus.Stopped) &&
                    !service.Status.Equals(ServiceControllerStatus.StopPending))
                {
                    return;
                }
                
                ChangeServiceStateTo(service, ServiceControllerStatus.Running, service.Start, logger);
            }
        }

        private static string GetServiceNameForExecutable(DeploymentContext context, string pathToExecutable)
        {
            string serviceName;
            Runspace runspace = RunspaceFactory.CreateRunspace();
            runspace.Open();
            var pipeline = runspace.CreatePipeline();
            var command =
                new Command(
                    @"Get-WmiObject -Class Win32_Service -Filter 'PathName LIKE ""%" +
                    Path.GetFileNameWithoutExtension(pathToExecutable).Replace(@"\", @"\\") + @"%""'", true);
            pipeline.Commands.Add(command);

            var results = pipeline.Invoke();
            runspace.Close();

            if (!results.Any())
            {
                return string.Empty;
            }

            serviceName = (string) results.First().Properties["Name"].Value;
            return serviceName;
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
