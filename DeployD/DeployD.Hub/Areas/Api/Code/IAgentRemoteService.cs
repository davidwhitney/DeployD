using System.Collections.Generic;
using DeployD.Hub.Areas.Api.Models;

namespace DeployD.Hub.Areas.Api.Code
{
    public interface IAgentRemoteService
    {
        List<PackageViewModel> ListPackages(string hostname);
        AgentViewModel GetAgentStatus(string hostname);
        void StartUpdatingAllPackages(string hostname, string version);
    }
}