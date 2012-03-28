using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO.Abstractions;
using log4net;

namespace Deployd.Core.AgentConfiguration
{
    public class AgentSettingsManager : IAgentSettingsManager
    {
        private readonly IFileSystem _fileSystem;
        private readonly ILog _log;
        public static Dictionary<string, string> ConfigurationDefaults { get; private set; }
        private IAgentSettings _settings = null;
        public IAgentSettings Settings
        {
            get
            {
                if (_settings == null)
                    _settings = LoadSettings();

                return _settings;
            }
        }

        public void UnloadSettings()
        {
            _settings = null;
        }

        public AgentSettingsManager(IFileSystem fileSystem, ILog log)
        {
            _fileSystem = fileSystem;
            _log = log;
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
                {"PackageSyncIntervalMs", "60000"},
                {"LatestDirectory", "~\\latest"},
                {"CacheDirectory", "~\\package_cache"},
                {"Tags",""}
            };
        }

        public IAgentSettings LoadSettings()
        {
            _log.Debug("Loading settings from app settings");

            return LoadSettings(ConfigurationManager.AppSettings);
        }

        public IAgentSettings LoadSettings(NameValueCollection settings)
        {
            var agentSettings = new AppSettings();
            ConfigureDefaults(settings, agentSettings);
            EnsurePathsExist(agentSettings);

            foreach(var setting in agentSettings)
            {
                _log.DebugFormat("{0} = {1}", setting.Key, setting.Value);
            }

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
