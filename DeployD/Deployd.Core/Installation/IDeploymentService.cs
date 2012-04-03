using System;
using System.Threading;
using NuGet;

namespace Deployd.Core.Installation
{
    public interface IDeploymentService
    {
        void InstallPackage(string packageId, string taskId, CancellationTokenSource cancellationToken, Action<ProgressReport> reportProgress);
        void InstallPackage(string packageId, string specificVersion, string taskId, CancellationTokenSource cancellationToken, Action<ProgressReport> reportProgress);
        void Deploy(string taskId, IPackage package, CancellationTokenSource cancellationToken, Action<ProgressReport> reportProgress);
        
    }
}