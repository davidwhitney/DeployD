using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO.Abstractions;

namespace Deployd.Core.AgentConfiguration
{
    public class AgentSettingsManager : IAgentSettingsManager
    {
        private readonly IFileSystem _fileSystem;
        public static Dictionary<string, string> ConfigurationDefaults { get; private set; }

        public AgentSettingsManager(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        static AgentSettingsManager()
        {
            ConfigurationDefaults = new Dictionary<string, string>
            {
                {"NuGetRepository", "~\\DebugPackageSource"},
                {"UnpackingLocation", "~\\app_unpack"},
                {"InstallationDirectory", "~\\app_root"},
                {"DeploymentEnvironment", "Production"},
                {"ConfigurationSyncIntervalMs", "60000"},
                {"PackageSyncIntervalMs", "60000"}
            };
        }

        public IAgentSettings LoadSettings()
        {
            return LoadSettings(ConfigurationManager.AppSettings);
        }

        public IAgentSettings LoadSettings(NameValueCollection settings)
        {
            var agentSettings = new AppSettings();
            ConfigureDefaults(settings, agentSettings);
            EnsurePathsExist(agentSettings);

            return agentSettings;
        }

        private void EnsurePathsExist(AppSettings agentSettings)
        {
            _fileSystem.EnsureDirectoryExists(agentSettings.InstallationDirectory);
            _fileSystem.EnsureDirectoryExists(agentSettings.UnpackingLocation);
        }

        private void ConfigureDefaults(NameValueCollection settings, IDictionary<string, string> agentSettings)
        {
            foreach (var keyValuePair in ConfigurationDefaults)
            {
                agentSettings[keyValuePair.Key] = SettingOrDefault(settings, keyValuePair.Key);
            }
        }

        private string SettingOrDefault(NameValueCollection settings, string key)
        {
            var value = (settings[key] ?? ConfigurationDefaults[key]) ?? string.Empty;
            return _fileSystem.MapVirtualPath(value);
        }
    }
}
