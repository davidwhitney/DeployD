namespace DeployD.Hub.Areas.Api.Models
{
    public class InstallTaskViewModel
    {
        public string[] messages { get; set; }
        public string status { get; set; }

        public string packageId { get; set; }

        public string version { get; set; }

        public string lastMessage { get; set; }
    }
}