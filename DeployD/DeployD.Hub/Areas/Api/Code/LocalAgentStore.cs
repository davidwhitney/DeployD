using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using DeployD.Hub.Areas.Api.Models;
using Deployd.Core;
using log4net;

namespace DeployD.Hub.Areas.Api.Code
{
    public class LocalAgentStore : IAgentStore
    {
        private readonly IAgentRemoteService _agentRemoteService;
        private readonly ILog _logger;
        public TimedSingleExecutionTask UpdateTask { get; private set; }

        public LocalAgentStore(IAgentRemoteService agentRemoteService, ILog logger)
        {
            int updateInterval;
            if (!int.TryParse(ConfigurationManager.AppSettings["UpdateInterval"], out updateInterval))
            {
                updateInterval = 5000;
            }
            _agentRemoteService = agentRemoteService;
            _logger = logger;
            UpdateTask = new TimedSingleExecutionTask(updateInterval, UpdateAgents, true);
            UpdateTask.Start(null);
        }

        private void UpdateAgentStatus(AgentViewModel agent)
        {
            AgentViewModel agentStatus = null;
            try
            {
                agentStatus = _agentRemoteService.GetAgentStatus(agent.id);
                agent.packages = agentStatus.packages;
                agent.currentTasks = agentStatus.currentTasks;
                agent.availableVersions = agentStatus.availableVersions;
                agent.environment = agentStatus.environment;
                agent.contacted = true;
            }
            catch (Exception ex)
            {
                _logger.Warn("Failed to get agent status", ex);
                agent.contacted = false;
            }
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