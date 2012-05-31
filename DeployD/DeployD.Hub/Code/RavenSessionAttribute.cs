using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Ninject;
using Raven.Client;

namespace DeployD.Hub.Code
{
    public class RavenSessionAttribute : System.Web.Mvc.ActionFilterAttribute
    {
        [Inject]
        public IDocumentStore DocumentStore { get; set; }
        public IDocumentSession RavenSession { get; protected set; }
        
        public override void OnActionExecuting(System.Web.Mvc.ActionExecutingContext filterContext)
        {
            RavenSession = DocumentStore.OpenSession();
        }

        public override void OnResultExecuted(System.Web.Mvc.ResultExecutedContext filterContext)
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