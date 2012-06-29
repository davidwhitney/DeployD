using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Deployd.Core.AgentConfiguration;
using Deployd.Core.Installation;
using Deployd.Core.PackageCaching;

namespace Deployd.Core
{
    public static class AgentStatusFactory
    {
        public static AgentStatusReport BuildStatus(ILocalPackageCache agentCache, IInstalledPackageArchive installCache, RunningInstallationTaskList runningTasks, IAgentSettingsManager settingsManager)
        {

            var status= new AgentStatusReport
                       {
                           packages = BuildPackageInformation(agentCache, installCache, runningTasks),
                           currentTasks = runningTasks != null ?
                                                    runningTasks.Select(t => new InstallTaskViewModel()
                                                                       {
                                                                           Messages = t.ProgressReports.Select(pr => pr.Message).ToArray(),
                                                                           Status = Enum.GetName(typeof(TaskStatus), t.Task.Status),
                                                                           PackageId = t.PackageId,
                                                                           Version = t.Version,
                                                                           LastMessage = t.ProgressReports.Count > 0 ? t.ProgressReports.LastOrDefault().Message : ""
                                                                       }).ToList()
                                                                       : new List<InstallTaskViewModel>(),
                           availableVersions = agentCache.AllCachedPackages() != null ? 
                                                agentCache.AllCachedPackages().Select(p => p.Version.ToString()).Distinct().OrderByDescending(s => s).ToList()
                                                : new List<string>(),
                           environment = settingsManager.Settings.DeploymentEnvironment,
                           updating = agentCache.Updating.Select(p=>string.Format("{0} {1}", p.Id, p.Version)).ToList()
                       };

            status.OutOfDate =
                status.packages.Any(p => p.OutOfDate);

            return status;
        }

        private static List<LocalPackageInformation> BuildPackageInformation(ILocalPackageCache agentCache, IInstalledPackageArchive installCache, RunningInstallationTaskList runningTasks)
        {
            var packages = agentCache.AvailablePackages;
            List<LocalPackageInformation> packageInformations = new List<LocalPackageInformation>();
            foreach(var packageId in packages)
            {
                var installedPackage = installCache.GetCurrentInstalledVersion(packageId);
                var latestAvailablePackage = agentCache.GetLatestVersion(packageId);
                var availablePackageVersions = agentCache.AvailablePackageVersions(packageId);
                IEnumerable<InstallationTask> currentTasks = null;
                if (runningTasks != null)
                {
                    currentTasks = runningTasks.Where(t => t.PackageId == packageId);
                }

                var packageInfo = new LocalPackageInformation();
                packageInfo.PackageId = packageId;


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