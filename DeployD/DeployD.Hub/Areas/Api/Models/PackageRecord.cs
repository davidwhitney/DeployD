namespace DeployD.Hub.Areas.Api.Models
{
    public class PackageRecord
    {
        public string PackageId { get; set; }
        public string[] AvailableVersions { get; set; }
        public bool Installed { get; set; }
        public string InstalledVersion { get; set; }
        public InstallTaskViewModel CurrentTask { get; set; }
    }
}