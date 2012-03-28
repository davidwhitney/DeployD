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
    public class AgentManager : IAgentManager
    {
        private readonly IAgentRepository _agentRepository;
        private readonly IAgentRemoteService _agentRemoteService;
        private readonly ILog _logger;
        public TimedSingleExecutionTask UpdateTask { get; private set; }

        public AgentManager(IAgentRepository agentRepository, IAgentRemoteService agentRemoteService, ILog logger)
        {
            int updateInterval;
            if (!int.TryParse(ConfigurationManager.AppSettings["UpdateInterval"], out updateInterval))
            {
                updateInterval = 5000;
            }
            _agentRepository = agentRepository;
            _agentRemoteService = agentRemoteService;
            _logger = logger;
            UpdateTask = new TimedSingleExecutionTask(updateInterval, StartUpdateOnAllAgents, true);
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
                _agentRepository.SaveOrUpdate(agent);
            }
            catch (Exception ex)
            {
                _logger.Warn("Failed to get agent status", ex);
                agent.Contacted = false;
            }
        }


        ~AgentManager()
        {
            UpdateTask.Stop();
        }

        public List<AgentRecord> ListAgents()
        {
            return _agentRepository.List();
        }

        public void RegisterAgentAndGetStatus(string hostname)
        {
            AgentRecord agent = new AgentRecord() { Hostname = hostname };
            new TaskFactory().StartNew(() => UpdateAgentStatus(agent));
            _agentRepository.SaveOrUpdate(agent);
        }

        public void StartUpdateOnAllAgents()
        {
            _agentRepository.List().ForEach(a => new TaskFactory().StartNew(() => UpdateAgentStatus(a)));
        }

        public void UnregisterAgent(string hostname)
        {
            _agentRepository.Remove(hostname);
        }
    }
}