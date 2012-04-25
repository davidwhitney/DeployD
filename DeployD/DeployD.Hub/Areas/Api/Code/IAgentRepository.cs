using System;
using System.Collections.Generic;
using DeployD.Hub.Areas.Api.Models;
using Deployd.Core;

namespace DeployD.Hub.Areas.Api.Code
{
    public interface IAgentRepository
    {
        void SaveOrUpdate(AgentRecord agent);
        void Remove(AgentRecord agent);
        void Remove(string hostname);
        List<AgentRecord> List();
        AgentRecord Get(Func<AgentRecord, bool> predicate );
        void SetApproved(string hostname);
    }
}