using System.Collections.Generic;
using System.Net;
using System.Web.Mvc;
using DeployD.Hub.Areas.Api.Code;
using DeployD.Hub.Areas.Api.Models;

namespace DeployD.Hub.Areas.Api.Controllers
{
    public class AgentController : Controller
    {
        private readonly IApiHttpChannel _httpChannel;
        private readonly IAgentStore _agentStore;

        public AgentController(IApiHttpChannel httpChannel, IAgentStore agentStore)
        {
            _httpChannel = httpChannel;
            _agentStore = agentStore;
        }

        //
        // GET: /Api/Agent/

        public ActionResult List()
        {
            return _httpChannel.RepresentationOf(_agentStore.ListAgents(), HttpContext);
        }

        public ActionResult Register(string hostname)
        {
            _agentStore.RegisterAgent(new AgentViewModel(){hostname=hostname});

            return new HttpStatusCodeResult((int) HttpStatusCode.Created);
        }
    }
}
