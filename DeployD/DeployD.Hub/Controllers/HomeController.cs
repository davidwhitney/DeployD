using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Ninject.Extensions.Logging;

namespace DeployD.Hub.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger _logger;

        public HomeController(ILogger logger)
        {
            _logger = logger;
        }

        //
        // GET: /Home/

        public ActionResult Index()
        {
            _logger.Info("home");

            return View();
        }

    }
}
