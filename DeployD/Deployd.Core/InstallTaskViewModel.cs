using System.Runtime.Serialization;

namespace Deployd.Core
{
    [DataContract(Name="installTask")]
    public class InstallTaskViewModel
    {
        [DataMember(Name="messages")]
        public string[] Messages { get; set; }
        [DataMember(Name="status")]
        public string Status { get; set; }
        [DataMember(Name = "packageId")]
        public string PackageId { get; set; }
        [DataMember(Name = "version")]
        public string Version { get; set; }
        [DataMember(Name = "lastMessage")]
        public string LastMessage { get; set; }
    }
}
