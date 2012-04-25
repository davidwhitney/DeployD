using System.Collections.Generic;
using DeployD.Hub.Areas.Api.Models;
using Deployd.Core;

namespace DeployD.Hub.Areas.Api.Code
{
    public interface IAgentRemoteService
    {
        List<PackageViewModel> ListPackages(string hostname);
        AgentStatusReport GetAgentStatus(string hostname);
        void StartUpdatingAllPackages(string hostname, string version);
        void StartUpdate(string hostname, string packageId, string version);
        List<LogListDto> ListPackagesWithLogs(string hostname);
        List<LogDto> ListLogsForPackage(string hostname, string packageId);
        LogFileDto GetLogFile(string hostname, string packageId, string filename);
    }
}