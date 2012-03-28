using System.Collections.Generic;
using DeployD.Hub.Areas.Api.Models;

namespace DeployD.Hub.Areas.Api.Code
{
    public interface IAgentManager
    {
        List<AgentRecord> ListAgents();
        void StartUpdateOnAllAgents();
        void RegisterAgentAndGetStatus(string hostname);
        void UnregisterAgent(string hostname);
    }
}