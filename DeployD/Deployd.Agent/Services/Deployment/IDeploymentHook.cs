using NuGet;

namespace Deployd.Agent.Services.Deployment
{
    public interface IDeploymentHook
    {
        void BeforeDeploy(DeploymentContext context);
        void Deploy(DeploymentContext context);
        void AfterDeploy(DeploymentContext context);
    }
}