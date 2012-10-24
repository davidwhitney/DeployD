using System.Web.Mvc;
using Ninject;
using Raven.Client;

namespace DeployD.Hub.Code
{
    public class RavenController : Controller
    {
        [Inject]
        public IDocumentStore DocumentStore { get; set; }
        public IDocumentSession RavenSession { get; protected set; }

        protected override void  OnActionExecuting(ActionExecutingContext filterContext)
        {
            RavenSession = DocumentStore.OpenSession();
        }

        protected override void  OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (filterContext.IsChildAction)
                return;

            using (RavenSession)
            {
                if (filterContext.Exception != null)
                    return;

                if (RavenSession != null)
                    RavenSession.SaveChanges();
            }
        }
    }
}