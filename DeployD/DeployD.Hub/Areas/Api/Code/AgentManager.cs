using System;
using System.Collections.Generic;
using System.Linq;
using DeployD.Hub.Areas.Api.Models;
using Deployd.Core;
using Ninject.Extensions.Logging;
using Raven.Abstractions.Exceptions;
using Raven.Client;
using log4net;

namespace DeployD.Hub.Areas.Api.Code
{
    public class AgentManager : IAgentManager
    {
        private readonly IDocumentSession _ravenSession;
        private readonly ILogger _logger;

        public AgentManager(IDocumentSession ravenSession, ILogger logger)
        {
            _ravenSession = ravenSession;
            _logger = logger;
        }

        public List<AgentRecord> ListAgents()
        {
            return _ravenSession.Query<AgentRecord>().ToList();
        }

        public AgentRecord RegisterAgent(string hostname)
        {
            if (_ravenSession.Load<AgentRecord>(hostname) != null)
                throw new InvalidOperationException("Agent already registered");

            var agent = new AgentRecord(hostname);
            _ravenSession.Store(agent);

            return agent;
        }

        public void UnregisterAgent(string hostname)
        {
            var agent = GetAgent(hostname);
            _ravenSession.Delete(agent);
        }

        public void ApproveAgent(string hostname)
        {
            var agent = GetAgent(hostname);
            agent.Approved = true;
            _ravenSession.SaveChanges();
        }

        public AgentRecord GetAgent(string hostname)
        {
            var agent = _ravenSession
                .Query<AgentRecord>()
                .Customize(q=>q.WaitForNonStaleResults(TimeSpan.FromSeconds(5)))
                .SingleOrDefault(a=>a.Id==hostname);
            return agent;
        }

        public void SetStatus(string hostname, AgentStatusReport agentStatus)
        {
            SetAgentStatus(agentStatus, GetAgent(hostname));
        }

        public void ReceiveStatus(string hostname, AgentStatusReport agentStatus)
        {
            var agent = GetAgent(hostname) ?? RegisterAgent(hostname);
            _ravenSession.Advanced.Refresh(agent);

            // set agent status and update
            SetAgentStatus(agentStatus, agent);
        }

        private void SetAgentStatus(AgentStatusReport agentStatus, AgentRecord agent)
        {
            if (agentStatus.packages != null)
            {

                _ravenSession.Advanced.GetEtagFor(agent);
                agent.Packages = agentStatus.packages
                    .Where(p => p.AvailableVersions != null)
                    .Select(p => new PackageViewModel
                                     {
                                         availableVersions = p.AvailableVersions.ToArray(),
                                         currentTask = p.CurrentTask,
                                         installedVersion = p.InstalledVersion,
                                         packageId = p.PackageId,
                                         installed = p.Installed
                                     }).ToList();
            }
            agent.CurrentTasks = agentStatus.currentTasks;
                agent.AvailableVersions = agentStatus.availableVersions;
            agent.Environment = agentStatus.environment;
            agent.Contacted = true;
            agent.LastContact = DateTime.Now;
            agent.Updating = agentStatus.updating;
            _ravenSession.Store(agent);
            System.Diagnostics.Debug.WriteLine("Update agent");

            try
            {
                _ravenSession.SaveChanges();
            } catch (ConcurrencyException)
            {
                _logger.Debug("agent status update was rejected because etag was old");
            }
        }
    }
}