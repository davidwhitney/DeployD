using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using DeployD.Hub.Areas.Api.Models;
using Deployd.Core;
using Raven.Client;

namespace DeployD.Hub.Areas.Api.Code
{
    public class RavenDbAgentRepository : IAgentRepository
    {
        private static object _lock=new object();
        private readonly IDocumentStore _documentStore;

        public RavenDbAgentRepository(IDocumentStore documentStore)
        {
            _documentStore = documentStore;
        }

        public void SaveOrUpdate(AgentRecord agent)
        {
            using (var session = _documentStore.OpenSession())
            {
                session.Advanced.UseOptimisticConcurrency = true;
                session.Store(agent);
                session.SaveChanges();
            }
        }

        public void Remove(AgentRecord agent)
        {
            using (var session = _documentStore.OpenSession())
            using (var transaction = new TransactionScope())
            {
                session.Delete(agent);
                session.SaveChanges();
                transaction.Complete();
            }
        }

        public void Remove(string hostname)
        {
            using (var session = _documentStore.OpenSession())
            {
                var agent = session.Query<AgentRecord>().SingleOrDefault(a => a.Hostname == hostname);
                if (agent == null)
                {
                    throw new ArgumentOutOfRangeException("hostname");
                }

                session.Delete(agent);
                session.SaveChanges();
            }

        }

        public List<AgentRecord> List()
        {
            using (var session = _documentStore.OpenSession())
            {
                return session.Query<AgentRecord>().ToList();
            }
        }

        public IEnumerable<AgentRecord> Where(Func<AgentRecord, bool> predicate )
        {
            using (var session = _documentStore.OpenSession())
            {
                return session.Query<AgentRecord>().Where(predicate);
            }
        }

        public AgentRecord Get(Func<AgentRecord, bool> predicate)
        {
            using (var session = _documentStore.OpenSession())
            {
                return session.Query<AgentRecord>().SingleOrDefault(predicate);
            }
        }

        public void SetApproved(string hostname)
        {
            using (var session = _documentStore.OpenSession())
            {
                var agent = session.Query<AgentRecord>().SingleOrDefault(a => a.Hostname == hostname);
                if (agent != null)
                {
                    agent.Approved = true;
                    session.SaveChanges();
                }
            }
        }
    }
}