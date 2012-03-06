using System;
using System.Collections.Generic;
using System.IO;
using Deployd.Agent.Services.Deployment.Hooks;
using Deployd.Core.AgentConfiguration;
using Deployd.Core.Hosting;
using NuGet;
using log4net;

namespace Deployd.Agent.Services.Deployment
{
    public class DeploymentService : IDeploymentService
    {
        private readonly IEnumerable<IDeploymentHook> _hooks;
        private readonly IAgentSettings _agentSettings;
        protected static readonly ILog Logger = LogManager.GetLogger("DeploymentService"); 
        public ApplicationContext AppContext { get; set; }

        public DeploymentService(IEnumerable<IDeploymentHook> hooks, IAgentSettings agentSettings)
        {
            _hooks = hooks;
            _agentSettings = agentSettings;
        }

        public void Start(string[] args)
        {
            Console.WriteLine("Started");
        }

        public void Stop()
        {
            Console.WriteLine("Stopped");
        }

        public void Deploy(IPackage package)
        {
            var frameworks = package.GetSupportedFrameworks();
            foreach(var framework in frameworks)
            {
                Logger.DebugFormat("package supports {0}", framework.FullName);
            }

            string outputPath = @"d:\temp\" + package.GetFullName();
            try
            {
                new PackageExtractor().Extract(package, outputPath);
            } catch (Exception ex)
            {
                Logger.Fatal("Could not extract package", ex);
            }

            string targetInstallationFolder = Path.Combine(@"d:\wwwcom", package.Title);
            var deploymentContext = new DeploymentContext(package, outputPath, targetInstallationFolder);
            BeforeDeploy(deploymentContext);

            PerformDeploy(deploymentContext);

            AfterDeploy(deploymentContext);
        }

        protected virtual void AfterDeploy(DeploymentContext context)
        {
            foreach(var hook in _hooks)
            {
                if (hook.HookValidForPackage(context))
                {
                    hook.AfterDeploy(context);
                }
            }
        }

        protected virtual void PerformDeploy(DeploymentContext context)
        {
            foreach (var hook in _hooks)
            {
                if (hook.HookValidForPackage(context))
                {
                    hook.Deploy(context);
                }
            }
        }

        protected virtual void BeforeDeploy(DeploymentContext context)
        {
            foreach (var hook in _hooks)
            {
                if (hook.HookValidForPackage(context))
                {
                    hook.BeforeDeploy(context);
                }
            }

        }
    }
}
