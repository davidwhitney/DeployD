using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DeployD.Hub.Areas.Api.Code;

namespace DeployD.Hub.Areas.Api.Controllers
{
    public class PackageController : Controller
    {
        private readonly IApiHttpChannel _apiHttpChannel;
        private readonly IPackageStore _packageStore;

        public PackageController(IApiHttpChannel apiHttpChannel, IPackageStore packageStore)
        {
            _apiHttpChannel = apiHttpChannel;
            _packageStore = packageStore;
        }

        //
        // GET: /Api/Package/

        public ActionResult List()
        {
            return _apiHttpChannel.RepresentationOf(_packageStore.ListAll(), HttpContext);
        }
        

        public ActionResult VersionList()
        {
            var packages = _packageStore.ListAll();

            if (packages == null)
                return _apiHttpChannel.RepresentationOf<VersionResult>(null, HttpContext);

            var allVersionSets = packages.Select(p => p.availableVersions);
            List<string> versions = new List<string>();
            foreach(var versionSet in allVersionSets)
            {
                versions.AddRange(versionSet);
            }
            var flattenedAndDistinct = versions.Distinct().OrderByDescending(v => v);

            return _apiHttpChannel.RepresentationOf(flattenedAndDistinct.Select(v => new VersionResult() { version = v }).ToArray(), HttpContext);
        }

        public class VersionResult
        {
            public string version { get; set; }
        }
    }
}
