using NuGet;

namespace Deployd.Agent.Services.Deployment
{
    public interface IDeploymentHook
    {
        bool BeforeDeploy(DeploymentContext context);
        bool Deploy(DeploymentContext context);
        bool AfterDeploy(DeploymentContext context);
    }
}