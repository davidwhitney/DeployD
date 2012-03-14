using System;
using System.Collections.Generic;
using System.Threading;
using Deployd.Core.Installation;
using NuGet;

namespace Deployd.Core.Deployment
{
    public interface IDeploymentService
    {
        void InstallPackage(string packageId, string taskId, CancellationTokenSource cancellationToken, Action<ProgressReport> reportProgress);
        void InstallPackage(string packageId, string specificVersion, string taskId, CancellationTokenSource cancellationToken, Action<ProgressReport> reportProgress);
        void Deploy(string taskId, IPackage package, CancellationTokenSource cancellationToken, Action<ProgressReport> reportProgress);
        
    }
}