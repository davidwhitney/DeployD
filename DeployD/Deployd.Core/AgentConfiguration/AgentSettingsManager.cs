using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using log4net;

namespace Deployd.Core.AgentConfiguration
{
    public class AgentSettingsManager : IAgentSettingsManager
    {
        private readonly IFileSystem _fileSystem;
        private readonly ILog _log;
        public static Dictionary<string, string> ConfigurationDefaults { get; private set; }
        private IAgentSettings _settings = null;
        private static object _fileLock=new object();
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
                {"BaseInstallationPath", "c:\\installations"},
                {"MsDeployServiceUrl", "localhost"},
                {"Tags",""},
                {"LogsDirectory", "~\\logs"},
            };
        }

        public IAgentSettings LoadSettings()
        {
            _log.Debug("Loading settings from app settings");

            var configuration = LocateOrCreateAgentConfiguration(_fileSystem);

            configuration = WatchForChanges(configuration);

            return LoadSettings(configuration.AppSettings.Settings);
        }

        private Configuration WatchForChanges(Configuration configuration)
        {
// watch for changes
            string agentConfigFilePath = Path.Combine(AgentSettings.AgentProgramDataPath, "agent.config");
            var configurationWatcher = new FileSystemWatcher(Path.GetDirectoryName(agentConfigFilePath),
                                                             Path.GetFileName(agentConfigFilePath));
            configurationWatcher.Changed += (sender, args) =>
                                                {
                                                    configurationWatcher.EnableRaisingEvents = false;
                                                    try
                                                    {
                                                        _log.Info("Configuration change detected - reloading");
                                                        configuration = LocateOrCreateAgentConfiguration(_fileSystem);
                                                        _settings = LoadSettings(configuration.AppSettings.Settings);
                                                    } finally
                                                    {
                                                        configurationWatcher.EnableRaisingEvents = true;
                                                    }
                                                };
            configurationWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.Size;
            configurationWatcher.EnableRaisingEvents = true;
            return configuration;
        }

        /// <summary>
        /// Ensures that an agent configuration file exists in the host's program data folder.
        /// This way re-installation will not override the in-use configuration settings.
        /// </summary>
        /// <param name="fileSystem"></param>
        /// <returns></returns>
        private Configuration LocateOrCreateAgentConfiguration(IFileSystem fileSystem)
        {
            lock (_fileLock)
            {
                string agentConfigFilePath = Path.Combine(AgentSettings.AgentProgramDataPath, "agent.config");
                if (!fileSystem.File.Exists(agentConfigFilePath))
                {
                    ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None)
                        .SaveAs(agentConfigFilePath, ConfigurationSaveMode.Full);
                }

                ExeConfigurationFileMap configMap = new ExeConfigurationFileMap();
                configMap.ExeConfigFilename = agentConfigFilePath;

                // wait up to a second for any file lock to end
                int waitCount = 10;
                while (waitCount-- > 0)
                {
                    try
                    {
                        return ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);
                    }
                    catch
                    {
                        System.Threading.Thread.Sleep(100);
                    }
                }
            }

            // all else fails just return the default configuration
            return ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        }

        public IAgentSettings LoadSettings(KeyValueConfigurationCollection settings)
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

        private void ConfigureDefaults(KeyValueConfigurationCollection settings, IDictionary<string, string> agentSettings)
        {
            foreach (var keyValuePair in ConfigurationDefaults)
            {
                agentSettings[keyValuePair.Key] = SettingOrDefault(settings, keyValuePair.Key);
            }
        }

        private string SettingOrDefault(KeyValueConfigurationCollection settings, string key)
        {
            var value = (settings[key].Value ?? ConfigurationDefaults[key]) ?? string.Empty;
            return _fileSystem.MapVirtualPath(value);
        }
    }
}
