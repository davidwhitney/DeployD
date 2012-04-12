using System;
using System.IO;
using Deployd.Core.AgentConfiguration;
using NuGet;
using log4net;
using log4net.Core;
using log4net.Repository.Hierarchy;

namespace Deployd.Core.Installation
{
    public class DeploymentContext
    {
        private readonly IPackage _package;
        private readonly IAgentSettings _agentSettings;
        private readonly string _workingFolder;
        private readonly string _installationTaskId;
        private readonly DateTime _contextCreateTime = DateTime.Now;
        private log4net.Appender.IAppender _appender;
        private readonly string _logAppenderName;
        private readonly string _logFileName;
        private readonly string _logDirectory;

        public DeploymentContext(IPackage package, IAgentSettings agentSettings, string workingFolder, string targetInstallationFolder, string installationTaskId)
        {
            _package = package;
            _agentSettings = agentSettings;
            _workingFolder = workingFolder;
            _installationTaskId = installationTaskId;
            TargetInstallationFolder = targetInstallationFolder;

            _logDirectory = _agentSettings.LogsDirectory.MapVirtualPath();
            _logFileName = string.Format("{0:dd-MM-yyyy-HH-mm-ss}.log", _contextCreateTime);
            _logAppenderName = string.Format("Install.{0}.{1:dd.MM.yyyy.HH.mm.ss}", _package.Id, _contextCreateTime);

            var layout = new log4net.Layout.PatternLayout();
            layout.ConversionPattern = "<div class='%-5level'>%d{dd-MM-yyyy HH:mm:ss} %logger - %message</div>";
            layout.ActivateOptions();

            var plainTextAppender = new log4net.Appender.FileAppender
                                        {
                                            Name = _logAppenderName,
                                            File = Path.Combine(AgentSettings.AgentProgramDataPath, Path.Combine(_logDirectory, Path.Combine(_package.Id, _logFileName))),
                                            AppendToFile = true,
                                            ImmediateFlush = true,
                                            Layout = layout,
                                            Threshold = Level.All,
                                            LockingModel = new log4net.Appender.FileAppender.MinimalLock()
                                        };
            plainTextAppender.ActivateOptions();

            _appender = plainTextAppender;
        }

        public string TargetInstallationFolder { get; set; }
        public string WorkingFolder { get { return _workingFolder; } }
        public IPackage Package { get { return _package; } }
        public string InstallationTaskId { get { return _installationTaskId; } }
        public string LogFileName { get { return _logFileName; } }

        public ILog GetLoggerFor<T>(T process)
        {
            var baseLogger = LogManager.GetLogger(_logAppenderName);

            ((Logger)baseLogger.Logger).AddAppender(_appender);
    
            return baseLogger;
        }

        public void RemoveAppender()
        {
            _appender.Close();
            var connectionAppender = (IAppenderAttachable) GetLoggerFor(this).Logger;
            connectionAppender.RemoveAppender(_appender);
            _appender = null;
        }
    }
}
