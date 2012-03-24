using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using DeployD.Hub.Areas.Api.Models;
using Deployd.Core;

namespace DeployD.Hub.Areas.Api.Code
{
    public class LocalAgentStore : IAgentStore
    {
        private readonly IAgentRemoteService _agentRemoteService;
        public TimedSingleExecutionTask UpdateTask { get; private set; }
        
        private void UpdateAgentStatus(AgentViewModel agent)
        {
            var agentStatus = _agentRemoteService.GetAgentStatus(agent.id);
            agent.packages = agentStatus.packages;
            agent.currentTasks = agentStatus.currentTasks;
            agent.availableVersions = agentStatus.availableVersions;
            agent.environment = agentStatus.environment;
        }

        public LocalAgentStore(IAgentRemoteService agentRemoteService)
        {
            _agentRemoteService = agentRemoteService;
            UpdateTask = new TimedSingleExecutionTask(10000, UpdateAgents, true);
            UpdateTask.Start(null);
        }

        ~LocalAgentStore()
        {
            UpdateTask.Stop();
        }

        private readonly List<AgentViewModel> _agents = new List<AgentViewModel>();
        public List<AgentViewModel> ListAgents()
        {
            UpdateAgents();
            return _agents;
        }

        public void RegisterAgent(AgentViewModel agent)
        {
            new TaskFactory().StartNew(() => UpdateAgentStatus(agent));
            _agents.Add(agent);
        }

        private void UpdateAgents()
        {
            _agents.ForEach(a => new TaskFactory().StartNew(() => UpdateAgentStatus(a)));
        }

        public void UnregisterAgent(string hostname)
        {
            if (!_agents.Any(a=>a.id==hostname))
            {
                throw new IndexOutOfRangeException("No agent found with given hostname");
            }

            _agents.RemoveAll(a=>a.id==hostname);
        }
    }

    public class GetPackageListResult
    {
        public IEnumerable<PackageViewModel> Packages { get; set; }
    }
}