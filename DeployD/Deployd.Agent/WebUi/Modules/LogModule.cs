using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Deployd.Core.AgentConfiguration;
using Deployd.Core.Hosting;
using Nancy;

namespace Deployd.Agent.WebUi.Modules
{
    public class LogModule : NancyModule
    {
        static readonly string LogDirectoryPath = Path.Combine(AgentSettings.AgentProgramDataPath, "installation_logs");
        public static Func<IIocContainer> Container { get; set; }

        public LogModule() : base("/log")
        {
            Get["/"] = x =>
            {
                var fileSystem = Container().GetType<IFileSystem>();
                var packageList = GetPackageLogDirectories(fileSystem);
                return this.ViewOrJson("logs/packages.cshtml", packageList);
            };

            Get["/{packageId}"] = x =>
            {
                var fileSystem = Container().GetType<IFileSystem>();
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
            var logFilePath = Path.Combine(LogDirectoryPath, Path.Combine(packageId, filename));
            
            if (!fileSystem.File.Exists(logFilePath))
            {
                throw new ArgumentException("Log not found", "filename");
            }

            var fileInfo = fileSystem.FileInfo.FromFileName(logFilePath);
            return new LogViewModel
            {
                LogFilePath = fileInfo.FullName,
                LogFileName = fileInfo.Name,
                PackageId = packageId,
                DateModified = fileInfo.LastWriteTime,
                LogContents = fileSystem.File.ReadAllText(fileInfo.FullName).Replace("\n","<br/>"),
                DateCreated = fileInfo.CreationTime
            };
        }

        private static List<string> GetPackageLogDirectories(IFileSystem fileSystem)
        {
            return !fileSystem.Directory.Exists(LogDirectoryPath)
                       ? new List<string>()
                       : fileSystem.Directory.GetDirectories(LogDirectoryPath).Select(Path.GetFileName).ToList();
        }

        private static PackageLogSetViewModel GetLogList(IFileSystem fileSystem, string packageId)
        {
            PackageLogSetViewModel viewModel = new PackageLogSetViewModel();
            viewModel.PackageId = packageId;
            viewModel.Logs = new List<LogViewModel>();
            var packageLogPath = Path.Combine(LogDirectoryPath, packageId);

            if (fileSystem.Directory.Exists(packageLogPath))
            {
                var logFiles = fileSystem.Directory.GetFiles(packageLogPath, "*.log", SearchOption.TopDirectoryOnly);

                if (logFiles != null)
                {
                    viewModel.Logs = logFiles.Select(f =>
                                                         {
                                                             var fileInfo1 = fileSystem.FileInfo.FromFileName(f);
                                                             return new LogViewModel()
                                                                        {
                                                                            LogFilePath = fileInfo1.FullName,
                                                                            LogFileName = fileInfo1.Name,
                                                                            PackageId = packageId,
                                                                            DateModified = fileInfo1.LastWriteTime,
                                                                            DateCreated = fileInfo1.CreationTime
                                                                        };
                                                         }
                        ).ToList();
                }
            }

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
