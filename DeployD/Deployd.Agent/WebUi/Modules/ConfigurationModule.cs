using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Deployd.Agent.Services.PackageDownloading;
using Deployd.Core.AgentConfiguration;
using Deployd.Core.Hosting;
using Nancy;
using Nancy.Responses;

namespace Deployd.Agent.WebUi.Modules
{
    public class ConfigurationModule : NancyModule
    {
        public static Func<IIocContainer> Container { get; set; }

        public ConfigurationModule() : base("/configuration")
        {
            Get["/"] = x =>
                           {
                               var agentWatchList = Container().GetType<IAgentWatchList>();
                               var agentSettings = Container().GetType<IAgentSettings>();

                               var configurationViewModel = new ConfigurationViewModel()
                                                                {
                                                                    WatchList = agentWatchList,
                                                                    Settings = agentSettings
                                                                };

                               return this.ViewOrJson("configuration/index.cshtml", configurationViewModel);
                           };

            Put["/watchList"] = x =>
                                    {
                                        var agentWatchListManager = Container().GetType<IAgentWatchListManager>();
                                        using (var streamReader = new StreamReader(Request.Body))
                                        {
                                            try
                                            {
                                                agentWatchListManager.SaveWatchList(streamReader.ReadToEnd());
                                            } catch (Exception ex)
                                            {
                                                return new TextResponse(HttpStatusCode.BadRequest, ex.Message);
                                            }
                                        }

                                        return new HeadResponse(new Response(){StatusCode = HttpStatusCode.Accepted});
                                    };
        }
    }

    public class ConfigurationViewModel
    {
        public IAgentWatchList WatchList { get; set; }

        public IAgentSettings Settings { get; set; }
    }
}
