using System;
using Deployd.Core.Hosting;

namespace Deployd.Agent
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

    }
}
