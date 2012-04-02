namespace Deployd.Core.AgentConfiguration
{
    public interface IAgentSettingsManager
    {
        IAgentSettings LoadSettings();
        IAgentSettings Settings { get; }
        void UnloadSettings();
    }
}