using System.Collections.Generic;
using DeployD.Hub.Areas.Api.Models;
using Deployd.Core;

namespace DeployD.Hub.Areas.Api.Code
{
    public interface IAgentManager
    {
        List<AgentRecord> ListAgents();
        AgentRecord RegisterAgent(string hostname);
        void UnregisterAgent(string hostname);
        void ApproveAgent(string hostname);
        AgentRecord GetAgent(string hostname);
        void SetStatus(string hostname, AgentStatusReport agentStatus);
        void ReceiveStatus(string hostname, AgentStatusReport agentStatus);
    }
}