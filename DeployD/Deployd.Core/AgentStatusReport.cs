using System.Collections.Generic;
using System.Runtime.Serialization;
using NuGet;

namespace Deployd.Core
{
    [DataContract(Name="agentStatus")]
    public class AgentStatusReport
    {
        [DataMember(Name="packages")]
        public List<LocalPackageInformation> packages { get; set; }

        [DataMember(Name = "currentTasks")]
        public List<InstallTaskViewModel> currentTasks { get; set; }

        [DataMember(Name = "availableVersions")]
        public List<string> availableVersions { get; set; }

        [DataMember(Name = "environment")]
        public string environment { get; set; }

        [DataMember(Name="updating")]
        public List<string> updating { get; set; }
    }
}