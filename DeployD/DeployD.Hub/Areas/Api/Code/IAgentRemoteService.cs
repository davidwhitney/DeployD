using System.Collections.Generic;
using DeployD.Hub.Areas.Api.Models;
using DeployD.Hub.Areas.Api.Models.Dto;

namespace DeployD.Hub.Areas.Api.Code
{
    public interface IAgentRemoteService
    {
        List<PackageViewModel> ListPackages(string hostname);
        AgentStatusReport GetAgentStatus(string hostname);
        void StartUpdatingAllPackages(string hostname, string version);
        void StartUpdate(string hostname, string packageId, string version);
    }
}