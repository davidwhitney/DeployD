using System.Collections.Generic;
using System.Runtime.Serialization;
using Deployd.Core;
using NuGet;

namespace Deployd.Agent.WebUi.Models
{
    [DataContract]
    public class PackageListViewModel
    {
        [IgnoreDataMember]
        [System.Xml.Serialization.XmlIgnore]
        public IList<LocalPackageInformation> Packages { get; set; }
        [IgnoreDataMember]
        [System.Xml.Serialization.XmlIgnore]
        public IList<InstallTaskViewModel> CurrentTasks { get; set; }
        [IgnoreDataMember]
        [System.Xml.Serialization.XmlIgnore]
        public IEnumerable<string> AvailableVersions { get; set; }
        [IgnoreDataMember]
        [System.Xml.Serialization.XmlIgnore]
        public List<string> Tags { get; set; }

        public string NugetRepository { get; set; }

        [IgnoreDataMember]
        [System.Xml.Serialization.XmlIgnore]
        public IList<IPackage> Updating { get; set; }

        public PackageListViewModel()
        {
            Packages = new List<LocalPackageInformation>();
            CurrentTasks = new List<InstallTaskViewModel>();
            Tags = new List<string>();
            AvailableVersions = new List<string>();
        }

    }
}
