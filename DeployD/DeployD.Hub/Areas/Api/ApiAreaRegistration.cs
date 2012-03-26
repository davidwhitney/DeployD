﻿using System.Web.Mvc;
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
            context.MapRoute("AgentMethod",
                "Api/agent/{id}/{action}",
                new { controller = "Agent", action = "Index" },
                new {httpMethod = new HttpMethodConstraint("GET","PUT","DELETE","POST")});

            context.MapRoute(
                "Api_default",
                "Api/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
