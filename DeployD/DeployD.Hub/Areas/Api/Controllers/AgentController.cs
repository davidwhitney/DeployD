using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
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

            AutoMapper.Mapper.CreateMap<List<AgentRecord>, List<AgentViewModel>>();
            AutoMapper.Mapper.CreateMap<AgentRecord, AgentViewModel>();
            AutoMapper.Mapper.CreateMap<PackageRecord, PackageViewModel>();
        }

        //
        // GET: /Api/Agent/
        [ActionName("List")]
        [HttpGet]
        public ActionResult List() // list
        {
            List<AgentRecord> agents = _agentStore.ListAgents();
            var viewModel = agents.Select(AutoMapper.Mapper.Map<AgentRecord, AgentViewModel>).ToList();

            return _httpChannel.RepresentationOf(viewModel, HttpContext);
        }

        [ActionName("Index")]
        [HttpGet]
        public ActionResult Index(string id) // list
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new HttpException((int)HttpStatusCode.BadRequest, "Invalid parameter", new ArgumentException("Invalid hostname", "id"));
            }

            AgentRecord agentRecord = _agentStore.ListAgents().SingleOrDefault(a => a.Hostname == id);
            var viewModel = AutoMapper.Mapper.Map<AgentRecord, AgentViewModel>(agentRecord);

            return _httpChannel.RepresentationOf(viewModel, HttpContext);
        }

        [AcceptVerbs("PUT")]
        [ActionName("Index")]
        public ActionResult IndexPost(string id)
        {
            _agentStore.RegisterAgent(id);

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

        [AcceptVerbs("POST")]
        [ActionName("applyVersions")]
        public ActionResult ApplyVersions(string id)
        {
            Response.Write("apply versions to " + id + "\n");
            var agentPackages = _agentRemoteService.ListPackages(id);
            foreach(var package in agentPackages)
            {
                string requestedVersion = Request.Form[package.packageId];
                if (!string.IsNullOrWhiteSpace(requestedVersion))
                {
                    Response.Write(string.Format("install version {0} of {1}\n", requestedVersion, package.packageId));
                    if (!package.availableVersions.Contains(requestedVersion))
                    {
                        Response.Write(string.Format("version {0} of {1} is not known\n", requestedVersion, package.packageId));
                    }
                    else if (package.installed && package.installedVersion == requestedVersion)
                    {
                        Response.Write("version is already installed\n");
                    } else
                    {
                        _agentRemoteService.StartUpdate(id, package.packageId, requestedVersion);
                    }
                }
            }

            return new HttpStatusCodeResult((int)HttpStatusCode.Accepted);

        }
    }
}
