using System.Collections.Generic;

namespace Deployd.Core.AgentManagement
{
    public interface IAgentActionsRepository
    {
        List<AgentAction> GetActionsForPackage(string packageId);
        AgentAction GetAction(string packageId, string action);
    }
}