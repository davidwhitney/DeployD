using System;
using System.IO;
using Deployd.Core.AgentConfiguration;
using NuGet;
using log4net;
using log4net.Core;
using log4net.Repository.Hierarchy;

namespace Deployd.Core.Deployment
{
    public class DeploymentContext
    {
        private readonly IPackage _package;
        private readonly string _workingFolder;
        private readonly string _installationTaskId;
        private readonly log4net.Appender.IAppender _logAppender=null;
        private DateTime _contextCreateTime = DateTime.Now;

        public DeploymentContext(IPackage package, string workingFolder, string targetInstallationFolder, string installationTaskId)
        {
            _package = package;
            _workingFolder = workingFolder;
            _installationTaskId = installationTaskId;
            TargetInstallationFolder = targetInstallationFolder;

            log4net.Layout.PatternLayout layout = new log4net.Layout.PatternLayout();
            layout.ConversionPattern = "%n%d{yyMMdd_hhmmss.fff};%-5level;%m";
            layout.Header = "Time;Level;Description;";

            string logFileName = string.Format("{0:dd-MM-yyyy-HH-mm-ss}.log", _contextCreateTime);
            var deploymentFileAppender = new log4net.Appender.FileAppender();
            deploymentFileAppender.File = Path.Combine(AgentSettings.AgentProgramDataPath, Path.Combine("installation_logs", Path.Combine(_package.Id, logFileName)));
            deploymentFileAppender.AppendToFile = true;
            deploymentFileAppender.ImmediateFlush = true;
            deploymentFileAppender.Layout = layout;
            deploymentFileAppender.Threshold = log4net.Core.Level.All;
            deploymentFileAppender.LockingModel = new log4net.Appender.FileAppender.MinimalLock();
            deploymentFileAppender.ActivateOptions();
            _logAppender = deploymentFileAppender;
        }

        public string TargetInstallationFolder { get; set; }
        public string WorkingFolder { get { return _workingFolder; } }
        public IPackage Package { get { return _package; } }
        public string InstallationTaskId { get { return _installationTaskId; } }

        public ILog GetLoggerFor<T>(T process)
        {
            var baseLogger = LogManager.GetLogger(typeof (T));

            ((Logger)baseLogger.Logger).AddAppender(_logAppender);
    
            return baseLogger;
        }
    }
}