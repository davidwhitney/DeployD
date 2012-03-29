using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using Deployd.Core.AgentConfiguration;
using Deployd.Core.Hosting;
using Nancy;
using log4net;

namespace Deployd.Agent.WebUi.Modules
{
    public class LogModule : NancyModule
    {
        private ILog _log = LogManager.GetLogger("InstallationsModule");
        static string _logDirectoryPath = Path.Combine(AgentSettings.AgentProgramDataPath, "installation_logs");
        public static Func<IIocContainer> Container { get; set; }

        public LogModule()
            : base("/log")
        {
            Get["/"] = x =>
                           {
                               var fileSystem = Container().GetType<System.IO.Abstractions.IFileSystem>();
                               List<string> packageList = GetPackageLogDirectories(fileSystem);
                               return this.ViewOrJson("logs/packages.cshtml", packageList);
                           };

            Get["/{packageId}"] = x =>
                                      {
                                          var fileSystem = Container().GetType<System.IO.Abstractions.IFileSystem>();
                                          PackageLogSetViewModel logList = GetLogList(fileSystem, x.packageId);
                                          return this.ViewOrJson("logs/package-logs.cshtml", logList);
                                      };
            Get["/{packageId}/{filename}"] = x =>
                                                 {
                                                     var fileSystem = Container().GetType<IFileSystem>();
                                                     LogViewModel log = GetLog(fileSystem, x.packageId, x.filename);
                                                     return this.ViewOrJson("logs/log-file.cshtml", log);
                                                 };
        }

        private LogViewModel GetLog(IFileSystem fileSystem, string packageId, string filename)
        {
            string logFilePath = Path.Combine(_logDirectoryPath, Path.Combine(packageId, filename));
            if (!fileSystem.File.Exists(logFilePath))
            {
                throw new ArgumentException("Log not found", "filename");
            }

            var fileInfo = fileSystem.FileInfo.FromFileName(logFilePath);
            return new LogViewModel()
            {
                LogFilePath = fileInfo.FullName,
                LogFileName = fileInfo.Name,
                PackageId = packageId,
                DateModified = fileInfo.LastWriteTime,
                LogContents = fileSystem.File.ReadAllText(fileInfo.FullName).Replace("\n","<br/>"),
                DateCreated = fileInfo.CreationTime
            };
        }

        private List<string> GetPackageLogDirectories(IFileSystem fileSystem)
        {
            if (!fileSystem.Directory.Exists(_logDirectoryPath))
                return new List<string>();

            return fileSystem.Directory.GetDirectories(_logDirectoryPath).Select(d => Path.GetFileName(d)).ToList();
        }

        private PackageLogSetViewModel GetLogList(IFileSystem fileSystem, string packageId)
        {
            string packageLogPath = Path.Combine(_logDirectoryPath, packageId);
            if (!fileSystem.Directory.Exists(packageLogPath))
            {
                return new PackageLogSetViewModel();
            }

            var logFiles = fileSystem.Directory.GetFiles(packageLogPath, "*.log", SearchOption.TopDirectoryOnly);

            PackageLogSetViewModel viewModel = new PackageLogSetViewModel();
            viewModel.PackageId = packageId;
            viewModel.Logs = logFiles.Select(f =>
                                       {
                                           var fileInfo = fileSystem.FileInfo.FromFileName(f);
                                            return new LogViewModel() {LogFilePath = fileInfo.FullName,
                                                LogFileName = fileInfo.Name,
                                                PackageId=packageId,
                                            DateModified = fileInfo.LastWriteTime,
                                            DateCreated = fileInfo.CreationTime};
            }
                ).ToList();

            return viewModel;
        }
    }

    public class PackageLogSetViewModel
    {
        public List<LogViewModel> Logs { get; set; }
        public string PackageId { get; set; }
    }

    public class LogViewModel
    {
        public string LogFilePath { get; set; }

        public DateTime DateModified { get; set; }

        public DateTime DateCreated { get; set; }

        public string LogFileName { get; set; }

        public string PackageId { get; set; }

        public string LogContents { get; set; }
    }
}
