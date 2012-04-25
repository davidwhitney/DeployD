using System.Collections.Generic;
using DeployD.Hub.Areas.Api.Models;
using Deployd.Core;

namespace DeployD.Hub.Areas.Api.Code
{
    public interface IAgentManager
    {
        List<AgentRecord> ListAgents();
        void StartUpdateOnAllAgents();
        void RegisterAgentAndGetStatus(string hostname);
        void UnregisterAgent(string hostname);
        void ApproveAgent(string id);
        AgentRecord GetAgent(string hostname);
        void SetStatus(string hostname, AgentStatusReport agentStatus);
    }
}