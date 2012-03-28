using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using DeployD.Hub.Areas.Api.Models;
using DeployD.Hub.Areas.Api.Models.Dto;
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

        private void UpdateAgentStatus(AgentRecord agent)
        {
            AgentStatusReport agentStatus = null;
            try
            {
                agentStatus = _agentRemoteService.GetAgentStatus(agent.Hostname);
                agent.Packages = agentStatus.packages;
                agent.CurrentTasks = agentStatus.currentTasks;
                agent.AvailableVersions = agentStatus.availableVersions;
                agent.Environment = agentStatus.environment;
                agent.Contacted = true;
            }
            catch (Exception ex)
            {
                _logger.Warn("Failed to get agent status", ex);
                agent.Contacted = false;
            }
        }


        ~LocalAgentStore()
        {
            UpdateTask.Stop();
        }

        private readonly List<AgentRecord> _agents = new List<AgentRecord>();
        public List<AgentRecord> ListAgents()
        {
            UpdateAgents();
            return _agents;
        }

        public void RegisterAgent(string hostname)
        {
            AgentRecord agent = new AgentRecord() { Hostname = hostname };
            new TaskFactory().StartNew(() => UpdateAgentStatus(agent));
            _agents.Add(agent);
        }

        private void UpdateAgents()
        {
            _agents.ForEach(a => new TaskFactory().StartNew(() => UpdateAgentStatus(a)));
        }

        public void UnregisterAgent(string hostname)
        {
            if (!_agents.Any(a=>a.Hostname==hostname))
            {
                throw new IndexOutOfRangeException("No agent found with given hostname");
            }

            _agents.RemoveAll(a=>a.Hostname==hostname);
        }
    }
}