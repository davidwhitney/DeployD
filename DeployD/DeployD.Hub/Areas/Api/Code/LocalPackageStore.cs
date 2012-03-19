using System.Collections.Generic;
using DeployD.Hub.Areas.Api.Models;

namespace DeployD.Hub.Areas.Api.Code
{
    public class LocalPackageStore : IPackageStore
    {
        private readonly List<PackageViewModel> _packages = new List<PackageViewModel>();

        public LocalPackageStore()
        {
            _packages.Add(new PackageViewModel(){id="GG.Web.Website", availableVersions = new[]{"1.0.0.0","1.0.0.1","1.0.0.2"}});
            _packages.Add(new PackageViewModel() { id = "GG.Api.Services", availableVersions = new[] { "1.0.0.0", "1.0.0.1", "1.0.0.2" } });
            _packages.Add(new PackageViewModel() { id = "GG.PaymentProcessing.Agent", availableVersions = new[] { "1.0.0.0", "1.0.0.1", "1.0.0.2" } });
            _packages.Add(new PackageViewModel() { id = "GG.Search.Indexing.Service", availableVersions = new[] { "1.0.0.0", "1.0.0.1", "1.0.0.2" } });
            _packages.Add(new PackageViewModel() { id = "GG.Api.Services.Sms", availableVersions = new[] { "1.0.0.0", "1.0.0.1", "1.0.0.2" } });
        }

        public IEnumerable<PackageViewModel> ListAll()
        {
            return _packages;
        }
    }
}