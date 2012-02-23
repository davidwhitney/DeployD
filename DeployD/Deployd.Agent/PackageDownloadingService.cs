using Deployd.Core.Hosting;

namespace Deployd.Agent
{
    public class PackageDownloadingService : IWindowsService
    {
        public ApplicationContext AppContext { get; set; }

        public void Start(string[] args)
        {
        }

        public void Stop()
        {
        }
    }
}
