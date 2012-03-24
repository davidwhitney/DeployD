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
        private readonly IAgentRemoteService _agentRemoteService;

        public AgentController(IApiHttpChannel httpChannel, IAgentStore agentStore, IAgentRemoteService agentRemoteService)
        {
            _httpChannel = httpChannel;
            _agentStore = agentStore;
            _agentRemoteService = agentRemoteService;
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

        [AcceptVerbs("POST")]
        [ActionName("UpdateAll")]
        public ActionResult UpdateAll(string version, string[] agentHostnames)
        {
            foreach (var hostname in agentHostnames)
            {
                _agentRemoteService.StartUpdatingAllPackages(hostname, version);
            }
            Response.Write("update all to version " + version);

            

            return new HttpStatusCodeResult((int)HttpStatusCode.Accepted);
        }
    }
}
