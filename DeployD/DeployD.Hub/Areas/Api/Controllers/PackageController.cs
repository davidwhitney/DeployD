using System;
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

    }
}
