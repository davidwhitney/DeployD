using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Deployd.Agent.Services.PackageDownloading;
using Deployd.Core;
using Deployd.Core.AgentConfiguration;
using Deployd.Core.Installation;
using Deployd.Core.PackageCaching;
using NuGet;

namespace Deployd.Agent.Services.HubCommunication
{
    public static class AgentStatusFactory
    {
        public static AgentStatusReport BuildStatus(IPackagesList availablePackages, ILocalPackageCache packageCache, IInstalledPackageArchive installCache, RunningInstallationTaskList runningTasks, IAgentSettingsManager settingsManager, ICurrentlyDownloadingList currentlyDownloadingList)
        {
            // copying these collections to variables because sometimes they get modified while building the status report object
            string[] updating = new string[currentlyDownloadingList != null ? currentlyDownloadingList.Count : 0];
            if (currentlyDownloadingList != null) currentlyDownloadingList.CopyTo(updating, 0);

            IPackage[] packages = new IPackage[availablePackages != null ? availablePackages.Count : 0];
            if (availablePackages != null) availablePackages.CopyTo(packages, 0);

            var watchedPackageList = availablePackages.GetWatched().ToList();
            IPackage[] watchedPackages = new IPackage[watchedPackageList != null ? watchedPackageList.Count : 0];
            if (watchedPackageList != null) watchedPackageList.CopyTo(watchedPackages);

            InstallationTask[] tasks=new InstallationTask[runningTasks != null ? runningTasks.Count : 0];
            if (runningTasks != null) runningTasks.CopyTo(tasks);

            var status= new AgentStatusReport
                       {
                           packages = BuildPackageInformation(watchedPackages, installCache, tasks),
                           currentTasks = tasks.Select(t => new InstallTaskViewModel()
                                                                       {
                                                                           Messages = t.ProgressReports.Select(pr => pr.Message).ToArray(),
                                                                           Status = Enum.GetName(typeof(TaskStatus), t.Task.Status),
                                                                           PackageId = t.PackageId,
                                                                           Version = t.Version,
                                                                           LastMessage = t.ProgressReports.Count > 0 ? t.ProgressReports.LastOrDefault().Message : ""
                                                                       }).ToList(),
                           availableVersions = packages.Select(p => p.Version.ToString()).Distinct().OrderByDescending(s => s).ToList(),
                           environment = settingsManager.Settings.DeploymentEnvironment,
                           updating = updating.ToList(),
                           isUpdating = currentlyDownloadingList.Downloading
                       };

            status.OutOfDate =
                status.packages.Any(p => p.OutOfDate);

            return status;
        }

        private static List<LocalPackageInformation> BuildPackageInformation(IEnumerable<IPackage> packages, IInstalledPackageArchive installCache, InstallationTask[] runningTasks)
        {
            List<LocalPackageInformation> packageInformations = new List<LocalPackageInformation>();
            var packagesById = packages.GroupBy(p=>p.Id);
            foreach (var packageVersions in packagesById)
            {
                var installedPackage = installCache.GetCurrentInstalledVersion(packageVersions.Key);
                var latestAvailablePackage = packageVersions.OrderByDescending(p => p.Version).FirstOrDefault();
                var availablePackageVersions = packageVersions.OrderByDescending(p=>p.Version).Select(p => p.Version.ToString());
                IEnumerable<InstallationTask> currentTasks = null;
                if (runningTasks != null)
                {
                    currentTasks = runningTasks.Where(t => t.PackageId == packageVersions.Key);
                }

                var packageInfo = new LocalPackageInformation();
                packageInfo.PackageId = packageVersions.Key;


                if (installedPackage != null)
                    packageInfo.InstalledVersion = installedPackage.Version.ToString();

                if (latestAvailablePackage != null)
                    packageInfo.LatestAvailableVersion = latestAvailablePackage.Version.ToString();

                if (availablePackageVersions != null)
                    packageInfo.AvailableVersions = availablePackageVersions.ToList();

                if (currentTasks != null)
                    packageInfo.CurrentTask = currentTasks.Select(t => new InstallTaskViewModel()
                    {
                        Messages = t.ProgressReports.Select(pr => pr.Message).ToArray(),
                        Status = Enum.GetName(typeof (TaskStatus), t.Task.Status),
                        PackageId = t.PackageId,
                        Version = t.Version,
                        LastMessage = t.ProgressReports.Count > 0 ? t.ProgressReports.LastOrDefault ().Message : ""
                    }).FirstOrDefault();

                packageInfo.OutOfDate = false;
                if (installedPackage != null)
                {
                    if (latestAvailablePackage != null)
                    {
                        packageInfo.OutOfDate = installedPackage.Version < latestAvailablePackage.Version;
                    } 
                } else
                {
                    if (latestAvailablePackage != null)
                    {
                        packageInfo.OutOfDate = true;
                    }
                }

                packageInformations.Add(packageInfo);
            }

            return packageInformations;

        }
    }
}