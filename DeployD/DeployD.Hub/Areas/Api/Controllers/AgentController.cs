using System.Collections.Generic;
using System.Linq;
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
        [ActionName("Index")]
        [HttpGet]
        public ActionResult Index(string id) // list
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return _httpChannel.RepresentationOf(_agentStore.ListAgents(), HttpContext);
            }
            else
            {
                return
                    _httpChannel.RepresentationOf(_agentStore.ListAgents().SingleOrDefault(a => a.id == id), HttpContext);
            }
        }

        [AcceptVerbs("PUT")]
        [ActionName("Index")]
        public ActionResult IndexPost(string id)
        {
            _agentStore.RegisterAgent(new AgentViewModel() { id = id });

            return new HttpStatusCodeResult((int) HttpStatusCode.Created);
        }

        [AcceptVerbs("DELETE")]
        [ActionName("Index")]
        public ActionResult IndexDelete(string id)
        {
            _agentStore.UnregisterAgent(id);
            return new HttpStatusCodeResult((int)HttpStatusCode.OK);
        }
    }
}
