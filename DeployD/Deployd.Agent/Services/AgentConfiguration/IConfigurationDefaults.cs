namespace Deployd.Agent.Services.AgentConfiguration
{
    public interface IConfigurationDefaults
    {
        string AgentConfigurationFile { get; }
        string AgentConfigurationFileLocation { get; }
    }
}