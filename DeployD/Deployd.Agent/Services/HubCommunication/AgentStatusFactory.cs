using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Deployd.Core;
using Deployd.Core.AgentConfiguration;
using Deployd.Core.Installation;
using Deployd.Core.PackageCaching;
using NuGet;

namespace Deployd.Agent.Services.HubCommunication
{
    public static class AgentStatusFactory
    {
        public static AgentStatusReport BuildStatus(IPackagesList availablePackages, ILocalPackageCache packageCache, IInstalledPackageArchive installCache, RunningInstallationTaskList runningTasks, IAgentSettingsManager settingsManager)
        {
            var status= new AgentStatusReport
                       {
                           packages = BuildPackageInformation(availablePackages.GetWatched(), installCache, runningTasks),
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
                           availableVersions = availablePackages != null ? 
                                                availablePackages.Select(p => p.Version.ToString()).Distinct().OrderByDescending(s => s).ToList()
                                                : new List<string>(),
                           environment = settingsManager.Settings.DeploymentEnvironment,
                       };

            status.OutOfDate =
                status.packages.Any(p => p.OutOfDate);

            return status;
        }

        private static List<LocalPackageInformation> BuildPackageInformation(IEnumerable<IPackage> packages, IInstalledPackageArchive installCache, RunningInstallationTaskList runningTasks)
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