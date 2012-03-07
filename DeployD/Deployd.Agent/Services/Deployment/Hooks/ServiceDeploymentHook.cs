using System;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using Deployd.Core.AgentConfiguration;

namespace Deployd.Agent.Services.Deployment.Hooks
{
    public class ServiceDeploymentHook : DeploymentHookBase
    {
        public override bool HookValidForPackage(DeploymentContext context)
        {
            return context.Package.Tags.ToLower().Contains("service");
        }

        public ServiceDeploymentHook(IAgentSettings agentSettings) : base(agentSettings)
        {
        }

        public override void BeforeDeploy(DeploymentContext context)
        {
            if (!EnvironmentIsValidForPackage(context))
            {
                return;
            }

            ShutdownRequiredServices(context);
        }

        private void ShutdownRequiredServices(DeploymentContext context)
        {
            using (var service = ServiceController.GetServices().SingleOrDefault(s => s.ServiceName == context.Package.Title))
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

                ChangeServiceStateTo(service, ServiceControllerStatus.Stopped, service.Stop);
            }
        }

        public override void Deploy(DeploymentContext context)
        {
            if (!EnvironmentIsValidForPackage(context))
            {
                return;
            }

            // services are installed in a '\services' subfolder
            context.TargetInstallationFolder = Path.Combine(@"d:\wwwcom\services", context.Package.Id);
            
            CopyAllFilesToDestination(context);
        }

        public override void AfterDeploy(DeploymentContext context)
        {
            if (!EnvironmentIsValidForPackage(context))
            {
                return;
            }

            // if no such service then install it
            using (var service = ServiceController.GetServices().SingleOrDefault(s => s.ServiceName == context.Package.Id))
            {
                if (service == null)
                {
                    var pathToExecutable = Path.Combine(context.TargetInstallationFolder, context.Package.Id + ".exe");
                    Logger.InfoFormat("Installing service {0} from {1}", context.Package.Title, pathToExecutable);

                    System.Configuration.Install.ManagedInstallerClass.InstallHelper(new[] {pathToExecutable});
                }

                // todo: recursively shut down dependent services
                if (!service.Status.Equals(ServiceControllerStatus.Stopped) &&
                    !service.Status.Equals(ServiceControllerStatus.StopPending))
                {
                    return;
                }
                
                ChangeServiceStateTo(service, ServiceControllerStatus.Running, service.Start);
            }
        }

        private void ChangeServiceStateTo(ServiceController service, ServiceControllerStatus verifyMeetsThisStatus, Action switchAction)
        {
            Logger.InfoFormat("Stopping service {0}", service.ServiceName);
            switchAction();

            var retryCount = 10; // wait 10 retries
            while (service.Status != verifyMeetsThisStatus && --retryCount > 0)
            {
                System.Threading.Thread.Sleep(100);
                service.Refresh();
            }

            Logger.InfoFormat("service is now {0}", service.Status);            
        }
    }
}
