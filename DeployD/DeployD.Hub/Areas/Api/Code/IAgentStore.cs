using System.Collections.Generic;
using DeployD.Hub.Areas.Api.Models;

namespace DeployD.Hub.Areas.Api.Code
{
    public interface IAgentStore
    {
        List<AgentViewModel> ListAgents();
        void RegisterAgent(AgentViewModel agent);
        void UnregisterAgent(string hostname);
    }
}