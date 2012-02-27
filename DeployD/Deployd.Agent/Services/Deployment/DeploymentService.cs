using System;
using System.Collections.Generic;
using Deployd.Core.Caching;
using Deployd.Core.Hosting;
using NuGet;
using log4net;

namespace Deployd.Agent.Services.Deployment
{
    public class DeploymentService : IWindowsService
    {
        private readonly IEnumerable<IDeploymentHook> _hooks;
        protected static readonly ILog Logger = LogManager.GetLogger("DeploymentService"); 
        public ApplicationContext AppContext { get; set; }

        public DeploymentService(IEnumerable<IDeploymentHook> hooks)
        {
            _hooks = hooks;
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

            var deploymentContext = new DeploymentContext(package, outputPath);
            BeforeDeploy(deploymentContext);

            PerformDeploy(deploymentContext);

            AfterDeploy(deploymentContext);
        }

        protected virtual void AfterDeploy(DeploymentContext context)
        {
            foreach(var hook in _hooks)
            {
                hook.AfterDeploy(context);
            }
        }

        protected virtual void PerformDeploy(DeploymentContext context)
        {
            foreach (var hook in _hooks)
            {
                hook.Deploy(context);
            }
        }

        protected virtual void BeforeDeploy(DeploymentContext context)
        {
            foreach (var hook in _hooks)
            {
                hook.BeforeDeploy(context);
            }

        }
    }
}
