using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Deployd.Agent.Services.Deployment
{
    // copies files to destination folder
    public abstract class DefaultDeploymentHook : IDeploymentHook
    {
        public virtual bool BeforeDeploy(DeploymentContext context){return false;}

        public virtual bool Deploy(DeploymentContext context)
        {
            // this is where file copy will occur

            return true;
        }

        public virtual bool AfterDeploy(DeploymentContext context){return false;}
    }
}
