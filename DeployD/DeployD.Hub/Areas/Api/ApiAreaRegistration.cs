using System.Web.Mvc;
using System.Web.Routing;

namespace DeployD.Hub.Areas.Api
{
    public class ApiAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Api";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute("ListAgents",
                             "api/agent",
                             new {controller = "Agent", action = "List"},
                             new {httpMethod = new HttpMethodConstraint("GET")});

            context.MapRoute("UpdateAgents",
                             "api/agent/updateAll",
                             new {controller = "Agent", action = "UpdateAll"},
                             new {httpMethod = new HttpMethodConstraint("POST")});
            context.MapRoute("RegisterAgent",
                             "api/agent/{hostname}",
                             new { controller = "Agent", action = "register" },
                             new { httpMethod = new HttpMethodConstraint("PUT") });

            context.MapRoute("AgentMethod",
                "Api/agent/{hostname}/{action}",
                new { controller = "Agent", action = "Index" },
                new {httpMethod = new HttpMethodConstraint("GET","PUT","DELETE","POST")});

            context.MapRoute("VersionList",
                             "api/versionlist",
                             new {controller = "Package", action = "VersionList"});

            // logs
            context.MapRoute("AgentLogFolders",
                             "api/log/{hostname}",
                             new {controller = "Log", action = "PackagesWithLogs"});
            context.MapRoute("AgentLogsForPackage",
                             "api/log/{hostname}/{packageId}",
                             new { controller = "Log", action = "ListForPackage" });
            context.MapRoute("AgentLog",
                             "api/log/{hostname}/{packageId}/{filename}",
                             new {controller = "Log", action = "LogFile"});

            context.MapRoute(
                "Api_default",
                "Api/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
