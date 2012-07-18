using System;

namespace Deployd.Core.Installation.Hooks
{
    public interface IDeploymentHook
    {
        bool HookValidForPackage(DeploymentContext context);
        void BeforeDeploy(DeploymentContext context, Action<ProgressReport> reportProgress );
        void Deploy(DeploymentContext context, Action<ProgressReport> reportProgress);
        void AfterDeploy(DeploymentContext context, Action<ProgressReport> reportProgress);
        string ProgressMessage { get; }
    }
}