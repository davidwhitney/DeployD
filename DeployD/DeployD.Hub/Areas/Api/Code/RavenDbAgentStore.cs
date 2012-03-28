using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DeployD.Hub.Areas.Api.Models;
using Raven.Client;

namespace DeployD.Hub.Areas.Api.Code
{
    public class RavenDbAgentStore : IAgentStore
    {
        private readonly IDocumentStore _documentStore;

        public RavenDbAgentStore(IDocumentStore documentStore)
        {
            _documentStore = documentStore;
        }

        public List<AgentRecord> ListAgents()
        {
            throw new NotImplementedException();
        }

        public void RegisterAgent(string hostname)
        {
            throw new NotImplementedException();
        }

        public void UnregisterAgent(string hostname)
        {
            throw new NotImplementedException();
        }
    }
}