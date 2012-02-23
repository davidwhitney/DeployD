using System;
using Deployd.Core.Hosting;
using Nancy;

namespace Deployd.Agent.WebUi
{
    public class WebUiModule : NancyModule
    {
        public static Func<IIocContainer> Container { get; set; }

        public WebUiModule()
        {
            Get["/"] = x => View["index.cshtml"];
        }
    }
}
