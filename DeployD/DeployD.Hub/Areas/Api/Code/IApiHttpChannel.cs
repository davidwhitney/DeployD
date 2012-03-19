using System.Web;
using System.Web.Mvc;

namespace DeployD.Hub.Areas.Api.Code
{
    public interface IApiHttpChannel
    {
        ActionResult RepresentationOf<T>(T resource, HttpContextBase httpContext);
    }
}