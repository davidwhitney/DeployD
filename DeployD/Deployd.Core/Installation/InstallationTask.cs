using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Deployd.Core.Installation
{
    [DataContract(Name="installationTask")]
    public class InstallationTask
    {
        public InstallationTask(string packageId, string version, string taskId, Task<InstallationResult> task, CancellationTokenSource cancellationTokenSource)
        {
            InstallationTaskId = taskId;
            PackageId = packageId;
            Version = version;
            Task = task;
            CancellationTokenSource = cancellationTokenSource;
            ProgressReports = new List<ProgressReport>();
            Errors = new List<Exception>();
            DateStarted = DateTime.Now;
            DateCompleted = DateTime.Now;
        }
        [DataMember(Name="task")]
        public Task<InstallationResult> Task { get; set; }
        private CancellationTokenSource CancellationTokenSource { get; set; }
        [DataMember(Name = "taskId")]
        public string InstallationTaskId { get; private set; }
        [DataMember(Name = "packageId")]
        public string PackageId { get; private set; }
        [DataMember(Name = "version")]
        public string Version { get; private set; }
        //[DataMember(Name = "progress")]
        [IgnoreDataMember]
        public List<ProgressReport> ProgressReports { get; private set; }
        [DataMember(Name = "lastMessage")]
        public string LastMessage { get { return ProgressReports.Count > 0 ? ProgressReports.Last().Message : ""; } set { }}
        [DataMember(Name = "hasErrors")]
        public bool HasErrors { get; set; }
        [IgnoreDataMember]
        public List<Exception> Errors { get; set; }
        [DataMember(Name = "logFileName")]
        public string LogFileName { get; set; }
        [DataMember(Name = "dateStarted")]
        public DateTime DateStarted { get; set; }
        [DataMember(Name = "dateCompleted")]
        public DateTime DateCompleted { get; set; }
        [DataMember(Name="installationResult")]
        public InstallationResult Result { get; set; }
    }
}