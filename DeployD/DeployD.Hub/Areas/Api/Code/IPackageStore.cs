using System.Collections.Generic;
using DeployD.Hub.Areas.Api.Models;

namespace DeployD.Hub.Areas.Api.Code
{
    public interface IPackageStore
    {
        IEnumerable<PackageViewModel> ListAll();
    }
}