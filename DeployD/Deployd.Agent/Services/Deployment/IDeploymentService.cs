using NuGet;

namespace Deployd.Agent.Services.Deployment
{
    public interface IDeploymentService
    {
        void InstallPackage(string packageId);
        void InstallPackage(string packageId, string specificVersion);
        void Deploy(IPackage package);
    }
}