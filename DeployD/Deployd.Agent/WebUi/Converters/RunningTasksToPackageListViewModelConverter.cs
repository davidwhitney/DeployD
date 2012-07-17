using System;
using System.Linq;
using System.Threading.Tasks;
using Deployd.Agent.WebUi.Models;
using Deployd.Core;
using Deployd.Core.AgentConfiguration;
using Deployd.Core.Installation;
using Deployd.Core.PackageCaching;

namespace Deployd.Agent.WebUi.Converters
{
    public static class RunningTasksToPackageListViewModelConverter
    {
        public static PackageListViewModel Convert(ILocalPackageCache cache, RunningInstallationTaskList runningTasks, 
            IInstalledPackageArchive installPackageArchive, CompletedInstallationTaskList completedTasks,
            IAgentSettings agentSettings, IPackagesList allPackagesList)
        {
            var model = new PackageListViewModel();

            var packagesById = allPackagesList.GetWatched().GroupBy(p => p.Id);
            foreach (var package in packagesById)
            {
                var packageInfo = new LocalPackageInformation
                                  {
                                      PackageId = package.Key
                                  };
                var latestVersion = package.OrderByDescending(p=>p.Version).FirstOrDefault();
                if (latestVersion != null)
                    packageInfo.LatestAvailableVersion = latestVersion.Version.ToString();


                var installedPackage = installPackageArchive.GetCurrentInstalledVersion(package.Key);
                packageInfo.InstalledVersion = installedPackage == null ? "0.0.0.0" : installedPackage.Version.ToString();

                /*packageInfo.LastInstallationTask =
                    completedTasks
                        .Where(t => t.PackageId == package.Key).OrderByDescending(t => t.LogFileName)
                        .FirstOrDefault();*/

                packageInfo.CurrentTask = runningTasks.Count > 0 ? runningTasks
                           .Where(t => t.PackageId == package.Key)
                           .Select(t =>
                                       {
                                           var lastOrDefault = t.ProgressReports.LastOrDefault();
                                           return lastOrDefault != null ? new InstallTaskViewModel
                                                                               {
                                                                                   Messages = t.ProgressReports.Select(pr => pr.Message).ToArray(),
                                                                                   Status = Enum.GetName(typeof(TaskStatus), t.Task.Status),
                                                                                   PackageId = t.PackageId,
                                                                                   Version = t.Version,
                                                                                   LastMessage = t.ProgressReports.Count > 0 ? lastOrDefault.Message : ""
                                                                               } : null;
                                       }).FirstOrDefault()
                           : null;

                packageInfo.AvailableVersions = package.OrderByDescending(x => x.Version).Select(x => x.Version.ToString()).ToList();

                if (latestVersion != null)
                {

                    packageInfo.Tags = latestVersion.Tags.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var tag in packageInfo.Tags)
                    {
                        if (!model.Tags.Any(t => t.Equals(tag.ToLower())))
                        {
                            model.Tags.Add(tag.ToLower());
                        }
                    }
                }
                else
                {
                    packageInfo.Tags = new string[0];
                }
                model.Packages.Add(packageInfo);

                model.Updating = cache.Updating;
            }

            model.CurrentTasks = runningTasks
                .Select(t =>
                            {
                                var progressReport = t.ProgressReports.LastOrDefault();
                                return progressReport != null
                                           ? new InstallTaskViewModel
                                                 {
                                                     Messages = t.ProgressReports.Select(pr => pr.Message).ToArray(),
                                                     Status = Enum.GetName(typeof (TaskStatus), t.Task.Status),
                                                     PackageId = t.PackageId,
                                                     Version = t.Version,
                                                     LastMessage =
                                                         t.ProgressReports.Count > 0 ? progressReport.Message : ""
                                                 }
                                           : null;
                            }).ToList();


            model.AvailableVersions =
                allPackagesList.Select(p => p.Version.ToString()).Distinct().OrderByDescending(s => s);

            model.NugetRepository = agentSettings.NuGetRepository;
            return model;
        }
    }
}
