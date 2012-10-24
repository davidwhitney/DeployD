using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Deployd.Agent.WebUi.Models;
using Deployd.Core;
using Deployd.Core.AgentConfiguration;
using Deployd.Core.Hosting;
using Nancy;

namespace Deployd.Agent.WebUi.Modules
{
    public class LogModule : NancyModule
    {
        public static Func<IIocContainer> Container { get; set; }

        public LogModule() : base("/log")
        {
            Get["/"] = x =>
            {
                var agentSettings = Container().GetType<IAgentSettings>();
                var fileSystem = Container().GetType<IFileSystem>();
                var packageList = GetPackageLogDirectories(fileSystem, agentSettings);
                packageList.Insert(0,"server");
                return this.ViewOrJson("logs/packages.cshtml", packageList);
            };

            Get["/{packageId}"] = x =>
            {
                var agentSettings = Container().GetType<IAgentSettings>();
                var fileSystem = Container().GetType<IFileSystem>();
                LogListViewModel logList = GetLogList(fileSystem, agentSettings, x.packageId);
                return this.ViewOrJson("logs/list.cshtml", logList);
            };

            Get["/{packageId}/{filename}"] = x =>
            {
                var agentSettings = Container().GetType<IAgentSettings>();
                var fileSystem = Container().GetType<IFileSystem>();
                LogViewModel log = GetLog(fileSystem, agentSettings, x.packageId, x.filename);
                return this.ViewOrJson("logs/log.cshtml", log);
            };

            Get["/server"] = x =>
            {
                var agentSettings = Container().GetType<IAgentSettings>();
                var fileSystem = Container().GetType<IFileSystem>();
                var viewModel = LoadServerLogList(fileSystem, agentSettings);
                return this.ViewOrJson("logs/list.cshtml", viewModel);
            };

            Get["/server/{filename}"] = x =>
                                            {
                string logFilename = string.IsNullOrWhiteSpace(x.filename)
                                        ? "DeployD.Agent.log"
                                        : x.filename;

                try
                {
                    var agentSettings = Container().GetType<IAgentSettings>();
                    var fileSystem = Container().GetType<IFileSystem>();
                    var viewModel = LoadLogViewModel(logFilename, fileSystem, agentSettings, true);
                    return this.ViewOrJson("logs/log.cshtml", viewModel);
                }catch(ArgumentException)
                {
                    return new NotFoundResponse();
                }
            };
        }

        private static string GetLogDirectory(IAgentSettings agentSettings)
        {
            return agentSettings.LogsDirectory.MapVirtualPath();
        }

        private static LogListViewModel LoadServerLogList(IFileSystem fileSystem, IAgentSettings agentSettings)
        {
            var logDirectoryPath = GetLogDirectory(agentSettings);
            var fileList = fileSystem.Directory.GetFiles(logDirectoryPath, "*.log", SearchOption.TopDirectoryOnly);

            return new LogListViewModel(fileList.Select(f => LoadLogViewModel(f, fileSystem, agentSettings, false))){Group="server"};
        }

        private static LogViewModel LoadLogViewModel(string logFilename, IFileSystem fileSystem, IAgentSettings agentSettings, bool includeContents, string subFolder=null)
        {
            string logDirectory = "";
            if (string.IsNullOrWhiteSpace(subFolder))
            {
                logDirectory = GetLogDirectory(agentSettings);
            } else
            {
                logDirectory = Path.Combine(GetLogDirectory(agentSettings), subFolder);
            }

            var logFilePath = Path.Combine(logDirectory, logFilename);
            
            if (!fileSystem.File.Exists(logFilePath))
            {
                throw new ArgumentOutOfRangeException("filename");
            }

            var fileInfo = fileSystem.FileInfo.FromFileName(logFilename);
            var viewModel = new LogViewModel()
                                {
                                    LogFileName = fileInfo.Name,
                                    Group = subFolder ?? "server",
                                    DateCreated = fileSystem.File.GetCreationTime(logFilePath),
                                    DateModified = fileSystem.File.GetLastWriteTime(logFilePath)
                                };

            if (includeContents)
            {
                using (var stream = new FileStream(logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var reader = new StreamReader(stream))
                {
                    viewModel.LogContents = reader.ReadToEnd();
                }
            }
            return viewModel;
        }

        private static LogViewModel GetLog(IFileSystem fileSystem, IAgentSettings agentSettings, string packageId, string filename)
        {
            return LoadLogViewModel(filename, fileSystem, agentSettings, true, packageId);
        }

        private static List<string> GetPackageLogDirectories(IFileSystem fileSystem, IAgentSettings agentSettings)
        {
            var logDirectory = GetLogDirectory(agentSettings);
            return !fileSystem.Directory.Exists(logDirectory)
                       ? new List<string>()
                       : fileSystem.Directory.GetDirectories(logDirectory).Select(Path.GetFileName).ToList();
        }

        private static LogListViewModel GetLogList(IFileSystem fileSystem, IAgentSettings agentSettings, string packageId)
        {
            var viewModel = new LogListViewModel {Group = packageId};
            var packageLogPath = Path.Combine(GetLogDirectory(agentSettings), packageId);

            if (fileSystem.Directory.Exists(packageLogPath))
            {
                var logFiles = fileSystem.Directory.GetFiles(packageLogPath, "*.log", SearchOption.TopDirectoryOnly);

                if (logFiles != null)
                {
                    viewModel.Logs.AddRange(logFiles.Select(f =>
                    {
                        var fileInfo1 = fileSystem.FileInfo.FromFileName(f);
                        return new LogViewModel
                                {
                                    LogFilePath = fileInfo1.FullName,
                                    LogFileName = fileInfo1.Name,
                                    Group = packageId,
                                    DateModified = fileInfo1.LastWriteTime,
                                    DateCreated = fileInfo1.CreationTime
                                };
                    })
                    .OrderByDescending(f=>f.DateModified));
                }
            }

            return viewModel;
        }
    }
}
