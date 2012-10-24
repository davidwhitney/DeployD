using System;
using System.Linq;
using System.Text;
using Deployd.Core.AgentConfiguration;
using Deployd.Core.Installation;

namespace Deployd.Core.AgentManagement
{
    public class AgentActionsService : IAgentActionsService
    {
        private readonly IAgentSettingsManager _agentSettingsManager;

        public AgentActionsService(IAgentSettingsManager agentSettingsManager)
        {
            _agentSettingsManager = agentSettingsManager;
        }

        public void RunAction(ActionTask action, Action<ProgressReport> reportProgress)
        {
            string scriptPath = 
                System.IO.Path.Combine(_agentSettingsManager.Settings.UnpackingLocation, action.ScriptPath);
            action.Log = 
                PowershellHelper.ExecutePowerShellScript(scriptPath, _agentSettingsManager.Settings)
                .ToString();
            
        }
    }

}
