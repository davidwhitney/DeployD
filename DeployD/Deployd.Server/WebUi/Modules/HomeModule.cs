using System;
using Deployd.Core.Caching;
using Deployd.Core.Hosting;
using Nancy;

namespace Deployd.Server.WebUi.Modules
{
    public class HomeModule : NancyModule
    {
        public static Func<IIocContainer> Container { get; set; }

        public HomeModule()
        {
            Get["/"] = x => View["index.cshtml"];
        }
    }
}
