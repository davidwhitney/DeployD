using System.Collections.Generic;
using DeployD.Hub.Areas.Api.Models;

namespace DeployD.Hub.Areas.Api.Code
{
    public interface IAgentRemoteService
    {
        IEnumerable<PackageViewModel> ListPackages(string hostname);
    }
}