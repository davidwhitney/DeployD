using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;

namespace Deployd.Core.AgentConfiguration
{
    public class AgentSettingsManager : IAgentSettingsManager
    {
        public static Dictionary<string, string> ConfigurationDefaults { get; private set; }

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

        private static void EnsurePathsExist(AppSettings agentSettings)
        {
            DirectoryHelpers.EnsureExists(agentSettings.InstallationDirectory);
            DirectoryHelpers.EnsureExists(agentSettings.UnpackingLocation);
        }

        private static void ConfigureDefaults(NameValueCollection settings, IDictionary<string, string> agentSettings)
        {
            foreach (var keyValuePair in ConfigurationDefaults)
            {
                agentSettings[keyValuePair.Key] = SettingOrDefault(settings, keyValuePair.Key);
            }
        }

        private static string SettingOrDefault(NameValueCollection settings, string key)
        {
            var value = (settings[key] ?? ConfigurationDefaults[key]) ?? string.Empty;
            return DirectoryHelpers.MapVirtualPath(value);
        }
    }
}
