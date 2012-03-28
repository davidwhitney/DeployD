using System;
using System.Collections.Generic;
using DeployD.Hub.Areas.Api.Models;

namespace DeployD.Hub.Areas.Api.Code
{
    public interface IAgentRepository
    {
        void SaveOrUpdate(AgentRecord agent);
        void Remove(AgentRecord agent);
        void Remove(string hostname);
        List<AgentRecord> List();
        AgentRecord Get(Func<List<AgentRecord>, AgentRecord> predicate );
    }
}