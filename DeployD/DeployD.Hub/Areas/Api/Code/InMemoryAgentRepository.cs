using System;
using System.Collections.Generic;
using System.Linq;
using DeployD.Hub.Areas.Api.Models;
using Deployd.Core;

namespace DeployD.Hub.Areas.Api.Code
{
    public class InMemoryAgentRepository : IAgentRepository
    {
        private List<AgentRecord> _agentList = new List<AgentRecord>(); 
        public void SaveOrUpdate(AgentRecord agent)
        {
            var existing = _agentList.SingleOrDefault(a => a.Hostname == agent.Hostname);
            if (existing != null)
            {
                _agentList[_agentList.IndexOf(existing)] = agent;
                existing = null;
            }
            else
            {
                _agentList.Add(agent);
            }
        }

        public void Remove(AgentRecord agent)
        {
            if (_agentList.Any(a => a.Hostname == agent.Hostname))
            {
                _agentList.RemoveAll(a => a.Hostname == agent.Hostname);
            }
        }
        public void Remove(string hostname)
        {
            if (_agentList.Any(a=>a.Hostname == hostname))
            {
                _agentList.RemoveAll(a => a.Hostname == hostname);
            }
        }

        public List<AgentRecord> List()
        {
            return _agentList;
        }

        public AgentRecord Get(Func<AgentRecord, bool> predicate )
        {
            return _agentList.SingleOrDefault(predicate);
        }

        public IEnumerable<AgentRecord> Where(Func<AgentRecord, bool> predicate)
        {
            throw new NotImplementedException();
        }

        public void SetApproved(string hostname)
        {
            var agent = _agentList.SingleOrDefault(a => a.Hostname == hostname);
            if (agent != null)
            {
                agent.Approved = true;
            }
        }
    }
}