using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Deployd.Core
{
    [DataContract(Name="agent")]
    public class AgentViewModel
    {
        [DataMember(Name="id")]
        public string id { get; set; }
        [DataMember(Name = "packages")]
        public List<PackageViewModel> packages { get; set; }
        [DataMember(Name = "availableVersions")]
        public List<string> availableVersions { get; set; }
        [DataMember(Name = "currentTasks")]
        public List<InstallTaskViewModel> currentTasks { get; set; }
        [DataMember(Name = "environment")]
        public string environment { get; set; }
        [DataMember(Name = "contacted")]
        public bool contacted { get; set; }
        [DataMember(Name = "approved")]
        public bool Approved { get; set; }
    }
}