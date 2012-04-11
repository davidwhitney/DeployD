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
        public Iis7MsDeployDeploymentHook(IAgentSettings agentSettings, IFileSystem fileSystem)
            : base(agentSettings, fileSystem)
        {
        }

        public override void BeforeDeploy(DeploymentContext context)
        {
            var logger = context.GetLoggerFor(this);
            var appPools = GetApplicationPoolsForWebsite(context.Package.Title);
            foreach (var appPool in appPools)
            {
                logger.InfoFormat("Stopping application pool {0}", appPool.Name);
                appPool.Stop();
            }
        }

        private IEnumerable<ApplicationPool> GetApplicationPoolsForWebsite(string websiteName)
        {
            var site = FindIis7Website(websiteName);
            var serverManager = new ServerManager();
            if (site != null)
            {
                foreach (var application in site.Applications)
                {
                    var appPool =
                        serverManager.ApplicationPools.SingleOrDefault(ap => ap.Name == application.ApplicationPoolName);
                    if (appPool != null)
                    {
                        yield return appPool;
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

        public override void AfterDeploy(DeploymentContext context)
        {
            var logger = context.GetLoggerFor(this);
            var appPools = GetApplicationPoolsForWebsite(context.Package.Title);
            foreach(var appPool in appPools)
            {
                logger.InfoFormat("Starting application pool {0}", appPool.Name);
                appPool.Start();
            }
        }

        public override bool HookValidForPackage(DeploymentContext context)
        {
            Site site;
            LocateMsDeploy(context.GetLoggerFor(this));
            var iis7SiteInstance = FindIis7Website(context.Package.Title);
            return context.Package.Tags.ToLower().Split(' ', ',', ';').Contains("website")
                   && !string.IsNullOrEmpty(MsWebDeployPath)
                   && TryFindIis7Website(context.Package.Id, out site);
        }
    }
}