namespace DeployD.Hub.Areas.Api.Models
{
    public class PackageViewModel
    {
        public string packageId { get; set; }
        public string[] availableVersions { get; set; }
        public bool installed { get; set; }
        public string installedVersion { get; set; }
        public InstallTaskViewModel currentTask { get; set; }
    }
}