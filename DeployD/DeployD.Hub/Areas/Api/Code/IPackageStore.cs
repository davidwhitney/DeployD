using System.Collections.Generic;
using DeployD.Hub.Areas.Api.Models;
using Deployd.Core;

namespace DeployD.Hub.Areas.Api.Code
{
    public interface IPackageStore
    {
        IEnumerable<PackageViewModel> ListAll();
    }
}