using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DeployD.Hub.Areas.Api.Models;

namespace DeployD.Hub.Areas.Api.Code
{
    public class LocalAgentStore : IAgentStore
    {
        private readonly IAgentRemoteService _agentRemoteService;

        public LocalAgentStore(IAgentRemoteService agentRemoteService)
        {
            _agentRemoteService = agentRemoteService;
        }

        private readonly List<AgentViewModel> _agents = new List<AgentViewModel>();
        public IEnumerable<AgentViewModel> ListAgents()
        {
            UpdateAgents();
            return _agents;
        }

        public void RegisterAgent(AgentViewModel agent)
        {
            agent.packages = _agentRemoteService.ListPackages(agent.hostname);
            _agents.Add(agent);
        }

        private void UpdateAgents()
        {
            _agents.ForEach(a => a.packages = _agentRemoteService.ListPackages(a.hostname));
        }

        public void UnregisterAgent(string hostname)
        {
            if (!_agents.Any(a=>a.hostname==hostname))
            {
                throw new IndexOutOfRangeException("No agent found with given hostname");
            }
            _agents.Remove(_agents.SingleOrDefault(a => a.hostname == hostname));
        }
    }
}