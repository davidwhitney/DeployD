using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DeployD.Hub.Areas.Api.Code;

namespace DeployD.Hub.Areas.Api.Controllers
{
    public class LogController : Controller
    {
        private readonly IAgentRemoteService _agentRemoteService;
        private readonly IApiHttpChannel _apiHttpChannel;

        public LogController(IAgentRemoteService agentRemoteService, IApiHttpChannel apiHttpChannel)
        {
            _agentRemoteService = agentRemoteService;
            _apiHttpChannel = apiHttpChannel;
        }

        //
        // GET: /Api/Log/

        public ActionResult PackagesWithLogs(string hostname)
        {
            var packageList = _agentRemoteService.ListPackagesWithLogs(hostname);

            return _apiHttpChannel.RepresentationOf(packageList, HttpContext);
        }

        public ActionResult ListForPackage(string hostname, string packageId)
        {
            var logList = _agentRemoteService.ListLogsForPackage(hostname, packageId);

            return _apiHttpChannel.RepresentationOf(logList, HttpContext);
        }

        public ActionResult LogFile(string hostname, string packageId, string filename)
        {
            var logFile = _agentRemoteService.GetLogFile(hostname, packageId, filename);
            return _apiHttpChannel.RepresentationOf(logFile, HttpContext);
        }
    }
}
