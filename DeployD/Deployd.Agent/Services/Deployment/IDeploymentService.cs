using System.Collections.Generic;
using Deployd.Agent.WebUi.Models;
using NuGet;

namespace Deployd.Agent.Services.Deployment
{
    public interface IDeploymentService
    {
        IList<LocalPackageInformation> AvailablePackages();
        void InstallPackage(string packageId);
        void InstallPackage(string packageId, string specificVersion);
        void Deploy(IPackage package);
    }
}