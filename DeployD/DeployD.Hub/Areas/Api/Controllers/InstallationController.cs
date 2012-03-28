using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DeployD.Hub.Areas.Api.Code;

namespace DeployD.Hub.Areas.Api.Controllers
{
    public class InstallationController : Controller
    {
        private readonly IAgentRemoteService _agentRemoteService;

        public InstallationController(IAgentRemoteService agentRemoteService)
        {
            _agentRemoteService = agentRemoteService;
        }

        //
        // GET: /Api/Deployment/

        public ActionResult Start(StartInstallationRequest request)
        {
            foreach(string agentHostname in request.Agents)
            {
                _agentRemoteService.StartUpdate(agentHostname, request.PackageId, request.Version);
            }
            return new HttpStatusCodeResult((int) HttpStatusCode.Accepted);
        }

    }

    public class StartInstallationRequest
    {
        public string PackageId { get; set; }
        public string Version { get; set; }
        public string[] Agents { get; set; }
    }
}
