using System.Collections.Generic;
using DeployD.Hub.Areas.Api.Models;

namespace DeployD.Hub.Areas.Api.Code
{
    public class AgentRemoteService : IAgentRemoteService
    {
        public IEnumerable<PackageViewModel> ListPackages(string hostname)
        {
            // todo: fake
            return new List<PackageViewModel>()
                       {
                           new PackageViewModel(){
                                                     id="GG.Web.Website", availableVersions = new string[]{"1.0.0.0","1.0.0.1","1.0.0.2","1.0.0.3"},installed=false},
                                                     new PackageViewModel(){id="GG.Api.Services", availableVersions = new string[]{"1.0.0.0"}, installed=true, installedVersion = "1.0.0.0"}
                       };
        }
    }
}