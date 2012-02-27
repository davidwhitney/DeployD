using System;
using Deployd.Core.Caching;
using Deployd.Core.Hosting;
using NuGet;

namespace Deployd.Agent.Services.Deployment
{
    public class DeploymentService : IWindowsService
    {
        public ApplicationContext AppContext { get; set; }
        
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
            
        }
    }
}
