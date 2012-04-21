using System;
using System.Linq;
using System.Text;
using Deployd.Core.AgentConfiguration;
using Deployd.Core.Installation;

namespace Deployd.Core.AgentManagement
{
    public class AgentActionsService : IAgentActionsService
    {
        private readonly IAgentSettings _agentSettings;

        public AgentActionsService(IAgentSettings agentSettings)
        {
            _agentSettings = agentSettings;
        }

        public void RunAction(ActionTask action, Action<ProgressReport> reportProgress)
        {
            string scriptPath = 
                System.IO.Path.Combine(_agentSettings.UnpackingLocation, action.ScriptPath);
            action.Log = 
                PowershellHelper.ExecutePowerShellScript(scriptPath, _agentSettings)
                .ToString();
            
        }
    }

}
