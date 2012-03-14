using System;
using System.Collections.Generic;
using System.Threading;

namespace Deployd.Core.Installation
{
    public class InstallationTaskQueue : Queue<InstallationTask>
    {
        public void Add(string packageId, string version = null)
        {
            Enqueue(new InstallationTask(packageId, version, Guid.NewGuid().ToString(), null, new CancellationTokenSource()));
        }
    }
}