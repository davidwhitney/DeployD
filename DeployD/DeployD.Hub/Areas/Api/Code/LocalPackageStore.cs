using System;
using System.Collections.Generic;
using System.Linq;
using DeployD.Hub.Areas.Api.Models;
using Deployd.Core;

namespace DeployD.Hub.Areas.Api.Code
{
    public class LocalPackageStore : IPackageStore
    {
        private readonly IAgentRemoteService _agentRemoteService;
        private readonly IAgentManager _agentManager;
        private List<PackageViewModel> _packages=null;
        private DateTime _lastRefresh = DateTime.Now;

        public LocalPackageStore(IAgentRemoteService agentRemoteService, IAgentManager agentManager)
        {
            _agentRemoteService = agentRemoteService;
            _agentManager = agentManager;
        }

        public IEnumerable<PackageViewModel> ListAll()
        {
            if ((_packages == null)
                || DateTime.Now.Subtract(_lastRefresh).TotalMinutes > 1)
            {
                List<AgentRecord> agents = _agentManager.ListAgents().ToList();
                if (agents.Count == 0)
                    return null;
                _packages = new List<PackageViewModel>();
                foreach(var agent in agents)
                {
                    _packages.AddRange(agent.Packages);
                }
            }

            return _packages;
        }
    }
}