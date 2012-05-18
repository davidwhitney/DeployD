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

        private void UpdateAgentStatus(AgentRecord agent, AgentStatusReport agentStatus)
        {
            agent.Packages = agentStatus.packages
                .Select(p => new PackageViewModel()
                                 {
                                     availableVersions = p.AvailableVersions.ToArray(), 
                                     currentTask = p.CurrentTask, 
                                     installedVersion = p.InstalledVersion, 
                                     packageId = p.PackageId,
                                     installed = p.Installed
                                 }).ToList();
            agent.CurrentTasks = agentStatus.currentTasks;
            agent.AvailableVersions = agentStatus.availableVersions;
            agent.Environment = agentStatus.environment;
            agent.Contacted = true;
            _agentRepository.SaveOrUpdate(agent);
        }

        private void UpdateAgentStatus(AgentRecord agent)
        {
            try
            {
                UpdateAgentStatus(agent, _agentRemoteService.GetAgentStatus(agent.Hostname));
            }
            catch (Exception ex)
            {
                _logger.InfoFormat("Agent {0} seems to be down", agent.Hostname);
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
            if (_agentRepository.List().Any(a=>a.Hostname == hostname))
            {
                throw new InvalidOperationException("Agent already registered");
            }

            AgentRecord agent = new AgentRecord() { Hostname = hostname };
            new TaskFactory().StartNew(() => UpdateAgentStatus(agent));
            _agentRepository.SaveOrUpdate(agent);
        }

        public void StartUpdateOnAllAgents()
        {
            //_agentRepository.List().ForEach(a => new TaskFactory().StartNew(() => UpdateAgentStatus(a)));
        }

        public void UnregisterAgent(string hostname)
        {
            _agentRepository.Remove(hostname);
        }

        public void ApproveAgent(string id)
        {
            _agentRepository.SetApproved(id);
        }

        public AgentRecord GetAgent(string hostname)
        {
            return _agentRepository.Where(a => a.Hostname == hostname).FirstOrDefault();
        }

        public void SetStatus(string hostname, AgentStatusReport agentStatus)
        {
            UpdateAgentStatus(_agentRepository.Get(a => a.Hostname == hostname), agentStatus);
        }
    }
}