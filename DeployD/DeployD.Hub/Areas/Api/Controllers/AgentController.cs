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
        private readonly IAgentManager _agentManager;
        private readonly IAgentRemoteService _agentRemoteService;

        public AgentController(IApiHttpChannel httpChannel, IAgentManager agentManager, IAgentRemoteService agentRemoteService)
        {
            _httpChannel = httpChannel;
            _agentManager = agentManager;
            _agentRemoteService = agentRemoteService;

            AutoMapper.Mapper.CreateMap<AgentRecord, AgentViewModel>().ForMember(viewModel=>viewModel.id, mo=>mo.MapFrom(record=>record.Hostname));
            AutoMapper.Mapper.CreateMap<PackageRecord, PackageViewModel>().ForMember(viewModel => viewModel.packageId, mo => mo.MapFrom(record => record.PackageId));
        }

        //
        // GET: /Api/Agent/
        [ActionName("List")]
        [HttpGet]
        public ActionResult List(bool? includeUnapproved) // list
        {
            if (!includeUnapproved.HasValue)
                includeUnapproved = false;

            _agentManager.StartUpdateOnAllAgents();
            List<AgentRecord> agents = _agentManager.ListAgents()
                .Where(a => a.Approved || includeUnapproved.Value) 
                    .ToList(); 
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

            AgentRecord agentRecord = _agentManager.ListAgents().SingleOrDefault(a => a.Hostname == id);
            var viewModel = AutoMapper.Mapper.Map<AgentRecord, AgentViewModel>(agentRecord);

            return _httpChannel.RepresentationOf(viewModel, HttpContext);
        }

        [AcceptVerbs("PUT")]
        [ActionName("Index")]
        public ActionResult IndexPost(string id)
        {
            _agentManager.RegisterAgentAndGetStatus(id);

            return new HttpStatusCodeResult((int) HttpStatusCode.Created);
        }

        [AcceptVerbs("DELETE")]
        [ActionName("Index")]
        public ActionResult IndexDelete(string id)
        {
            _agentManager.UnregisterAgent(id);
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

        [AcceptVerbs("PUT")]
        [ActionName("register")]
        public ActionResult Register(string hostname)
        {
            var agent = _agentManager.GetAgent(hostname);
            if (agent != null)
                return new HttpStatusCodeResult((int)HttpStatusCode.Conflict);

            _agentManager.RegisterAgentAndGetStatus(hostname);
            return new HttpStatusCodeResult((int)HttpStatusCode.Created);
        }

        [AcceptVerbs("POST")]
        [ActionName("approve")]
        public ActionResult Approve(string id)
        {
            _agentManager.ApproveAgent(id);
            return new HttpStatusCodeResult((int)HttpStatusCode.Accepted);
        }

        [AcceptVerbs("GET")]
        [ActionName("ping")]
        public ActionResult Ping(string hostname)
        {
            var agent = _agentManager.GetAgent(hostname);
            if (agent != null)
            {
                if (agent.Approved)
                {
                    return new HttpStatusCodeResult((int) HttpStatusCode.OK);
                } else
                {
                    return new HttpStatusCodeResult((int)HttpStatusCode.Unauthorized);
                }
            }
            return new HttpNotFoundResult();
        }
    }
}
