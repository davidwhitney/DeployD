using System.Collections.Generic;

namespace Deployd.Agent.WebUi.Models
{
    public class PackageVersionsViewModel : List<string>
    {
        public string PackageName { get; set; }

        public PackageVersionsViewModel(string packageName, IEnumerable<string> inner)
        {
            PackageName = packageName;
            AddRange(inner);
        }
    }
}