namespace Deployd.Core.AgentConfiguration
{
    public interface IAgentInstance
    {
        string MachineName { get; set; }
        string IpAddress { get; set; }
        string DeploymentEnvironment { get; set; }
    }
}