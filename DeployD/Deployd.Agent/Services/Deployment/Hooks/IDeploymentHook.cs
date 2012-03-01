namespace Deployd.Agent.Services.Deployment.Hooks
{
    public interface IDeploymentHook
    {
        bool HookValidForPackage(DeploymentContext context);
        void BeforeDeploy(DeploymentContext context);
        void Deploy(DeploymentContext context);
        void AfterDeploy(DeploymentContext context);
    }
}