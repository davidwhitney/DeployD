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
            var appSettings = new AppSettings(settings);
            
            ConfigureDefaults(appSettings);
            EnsurePathsExist(appSettings);

            return appSettings;
        }

        private static void EnsurePathsExist(AppSettings agentSettings)
        {
            DirectoryHelpers.EnsureExists(agentSettings.InstallationDirectory);
            DirectoryHelpers.EnsureExists(agentSettings.UnpackingLocation);
        }

        private static void ConfigureDefaults(IDictionary<string, string> agentSettings)
        {
            foreach (var keyValuePair in ConfigurationDefaults)
            {
                agentSettings[keyValuePair.Key] = SettingOrDefault(keyValuePair.Key);
            }
        }

        private static string SettingOrDefault(string key)
        {
            var value = (ConfigurationManager.AppSettings[key] ?? ConfigurationDefaults[key]) ?? string.Empty;
            return DirectoryHelpers.MapVirtualPath(value);
        }
    }
}
