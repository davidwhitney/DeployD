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
        private readonly string _workingFolder;
        private readonly string _installationTaskId;
        private readonly DateTime _contextCreateTime = DateTime.Now;
        private log4net.Appender.IAppender _appender;
        private readonly string _logAppenderName;

        public DeploymentContext(IPackage package, string workingFolder, string targetInstallationFolder, string installationTaskId)
        {
            _package = package;
            _workingFolder = workingFolder;
            _installationTaskId = installationTaskId;
            TargetInstallationFolder = targetInstallationFolder;

            var logFileName = string.Format("{0:dd-MM-yyyy-HH-mm-ss}.log", _contextCreateTime);
            _logAppenderName = string.Format("Install.{0}.{1:dd.MM.yyyy.HH.mm.ss}", _package.Id, _contextCreateTime);

            var layout = new log4net.Layout.PatternLayout();
            layout.ConversionPattern = "%d{dd-MM-yyyy HH:mm:ss} [%thread] %-5level %logger - %message%newline";
            layout.Header = "Time;Level;Description;";

            var plainTextAppender = new log4net.Appender.FileAppender
                                        {
                                            Name = _logAppenderName,
                                            File = Path.Combine(AgentSettings.AgentProgramDataPath, Path.Combine("installation_logs", Path.Combine(_package.Id, logFileName))),
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