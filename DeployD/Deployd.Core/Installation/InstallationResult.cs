using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Deployd.Core.Installation
{
    [DataContract(Name="installationResult")]
    public class InstallationResult
    {
        [DataMember(Name="failed", EmitDefaultValue = true)]
        public bool Failed { get; set; }
    }
}