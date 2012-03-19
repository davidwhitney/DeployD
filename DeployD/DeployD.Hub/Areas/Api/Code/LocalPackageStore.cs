using System;
using System.Collections.Generic;
using System.Linq;
using DeployD.Hub.Areas.Api.Models;

namespace DeployD.Hub.Areas.Api.Code
{
    public class LocalPackageStore : IPackageStore
    {
        private readonly IAgentRemoteService _agentRemoteService;
        private readonly IAgentStore _agentStore;
        private List<PackageViewModel> _packages=null;
        private DateTime _lastRefresh = DateTime.Now;

        public LocalPackageStore(IAgentRemoteService agentRemoteService, IAgentStore agentStore)
        {
            _agentRemoteService = agentRemoteService;
            _agentStore = agentStore;
        }

        public IEnumerable<PackageViewModel> ListAll()
        {
            if ((_packages == null)
                || DateTime.Now.Subtract(_lastRefresh).TotalMinutes > 1)
            {
                List<AgentViewModel> agents = _agentStore.ListAgents().ToList();
                if (agents.Count == 0)
                    return null;
                _packages = new List<PackageViewModel>();
                foreach(var agent in agents)
                {
                    _packages.AddRange(agent.packages);
                }
            }

            return _packages;
        }
    }
}