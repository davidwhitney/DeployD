namespace DeployD.Hub.Areas.Api.Models
{
    public class AgentViewModel
    {
        public string hostname { get; set; }
        public PackageViewModel[] packages { get; set; }
    }
}