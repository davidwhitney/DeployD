using System.Collections.Generic;
using System.Web.Mvc;
using DeployD.Hub.Areas.Api.Code;
using DeployD.Hub.Areas.Api.Models;

namespace DeployD.Hub.Areas.Api.Controllers
{
    public class AgentController : Controller
    {
        private readonly IApiHttpChannel _httpChannel;

        public AgentController(IApiHttpChannel httpChannel)
        {
            _httpChannel = httpChannel;
        }

        //
        // GET: /Api/Agent/

        public ActionResult Index()
        {
            var agents = new AgentViewModel
                             {
                                 hostname = "server1",
                                 packages = new PackageViewModel []
                                                {
                                                    new PackageViewModel
                                                        {
                                                            id = "package1",
                                                            availableVersions = new[] {"1.0.0.0", "1.0.0.1"},
                                                            installed = false,
                                                            installedVersion = ""
                                                        },
                                                    new PackageViewModel
                                                        {
                                                            id = "package2",
                                                            availableVersions = new[] {"1.0.0.0", "1.0.0.1"},
                                                            installed = true,
                                                            installedVersion = "1.0.0.0"
                                                        }
                                                }
                             };

            return _httpChannel.RepresentationOf(agents, HttpContext);
        }
    }
}
