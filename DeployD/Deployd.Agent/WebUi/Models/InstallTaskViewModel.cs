namespace Deployd.Agent.WebUi.Models
{
    public class InstallTaskViewModel
    {
        public string[] Messages { get; set; }
        public string Status { get; set; }
        public string PackageId { get; set; }
        public string Version { get; set; }
        public string LastMessage { get; set; }
    }
}
