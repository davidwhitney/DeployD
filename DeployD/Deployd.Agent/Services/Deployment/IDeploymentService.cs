using Deployd.Core.Hosting;
using NuGet;

namespace Deployd.Agent.Services.Deployment
{
    public interface IDeploymentService : IWindowsService
    {
        void Deploy(IPackage package);
    }
}