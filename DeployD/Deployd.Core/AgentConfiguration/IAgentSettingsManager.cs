namespace Deployd.Core.AgentConfiguration
{
    public interface IAgentSettingsManager
    {
        IAgentSettings LoadSettings();
    }
}