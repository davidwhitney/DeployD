using System;
using System.Collections.Generic;
using Deployd.Core;

namespace DeployD.Hub.Areas.Api.Models
{
    public class AgentRecord
    {
        public string Id { get; set; }
        public string Hostname { get; set; }
        public List<PackageViewModel> Packages { get; set; }
        public List<string> AvailableVersions { get; set; }
        public List<InstallTaskViewModel> CurrentTasks { get; set; }
        public string Environment { get; set; }
        public bool Contacted { get; set; }
        public bool Approved { get; set; }
        public DateTime LastContact { get; set; }

        public List<string> Updating { get; set; }

        public AgentRecord(string hostname)
        {
            Packages = new List<PackageViewModel>();
            CurrentTasks = new List<InstallTaskViewModel>();
            AvailableVersions = new List<string>();
            Id = hostname;
        }

    }
}