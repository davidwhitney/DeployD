using System.Collections.Generic;
using DeployD.Hub.Areas.Api.Models;

namespace DeployD.Hub.Areas.Api.Code
{
    public interface IAgentStore
    {
        List<AgentRecord> ListAgents();
        void RegisterAgent(string hostname);
        void UnregisterAgent(string hostname);
    }
}