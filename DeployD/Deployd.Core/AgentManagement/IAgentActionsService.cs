using System;
using Deployd.Core.Installation;

namespace Deployd.Core.AgentManagement
{
    public interface IAgentActionsService
    {
        void RunAction(ActionTask action, Action<ProgressReport> reportProgress);
    }
}