using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Deployd.Core.AgentConfiguration;
using Microsoft.Web.Administration;
using log4net;

namespace Deployd.Core.Installation.Hooks
{
    public class Iis7MsDeployDeploymentHook : IisMsDeployDeploymentHook
    {
        public Iis7MsDeployDeploymentHook(IAgentSettingsManager agentSettingsManager, IFileSystem fileSystem)
            : base(agentSettingsManager, fileSystem)
        {
        }

        public override void BeforeDeploy(DeploymentContext context, Action<ProgressReport> reportProgress)
        {
            reportProgress(new ProgressReport(context, GetType(), "Stopping application pool(s)")); 
            
            var logger = context.GetLoggerFor(this);
            var appPools = GetApplicationPoolsForWebsite(context.Package.Title);
            foreach (var appPool in appPools)
            {
                if (appPool.State == ObjectState.Started)
                {
                    logger.InfoFormat("Stopping application pool {0}", appPool.Name);
                    appPool.Stop();
                }
            }
        }

        private IEnumerable<ApplicationPool> GetApplicationPoolsForWebsite(string websiteName)
        {
            Site site = null;
            if (TryFindIis7Website(websiteName, out site))
            {
                var serverManager = new ServerManager();
                if (site != null)
                {
                    foreach (var application in site.Applications)
                    {
                        var appPool =
                            serverManager.ApplicationPools.SingleOrDefault(
                                ap => ap.Name == application.ApplicationPoolName);
                        if (appPool != null)
                        {
                            yield return appPool;
                        }
                    }
                }
            }

        }

        private static Site FindIis7Website(string websiteName)
        {
            return new ServerManager().Sites.SingleOrDefault(s => s.Name == websiteName);
        }

        private static bool TryFindIis7Website(string websiteName, out Site site)
        {
            try
            {
                site = FindIis7Website(websiteName);
                return true;
            } catch
            {
                site = null;
                return false;
            }
        }

        public override void AfterDeploy(DeploymentContext context, Action<ProgressReport> reportProgress)
        {
            var logger = context.GetLoggerFor(this);
            reportProgress(new ProgressReport(context, GetType(), "Starting application pool(s)")); 
            var appPools = GetApplicationPoolsForWebsite(context.Package.Title);
            foreach(var appPool in appPools)
            {
                if (appPool.State == ObjectState.Stopped)
                {
                    logger.InfoFormat("Starting application pool {0}", appPool.Name);
                    appPool.Start();
                }
            }
        }

        public override bool HookValidForPackage(DeploymentContext context)
        {
            Site site;
            LocateMsDeploy(context.GetLoggerFor(this));
            Site iis7SiteInstance = null;
            if (TryFindIis7Website(context.Package.Title, out iis7SiteInstance))
            {
                return context.Package.Tags.ToLower().Split(' ', ',', ';').Contains("website")
                       && !string.IsNullOrEmpty(MsWebDeployPath)
                       && TryFindIis7Website(context.Package.Id, out site);
            } else
            {
                return false;
            }
        }
    }
}