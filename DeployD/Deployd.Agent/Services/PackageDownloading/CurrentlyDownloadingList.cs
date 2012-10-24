using System;
using System.Collections.Generic;

namespace Deployd.Agent.Services.PackageDownloading
{
    public interface ICurrentlyDownloadingList : IList<string>
    {
        bool Downloading { get; }
    }

    public class CurrentlyDownloadingList : List<string>, ICurrentlyDownloadingList
    {
        private DateTime _showDownloadingUntil = DateTime.MinValue;
        public bool Downloading { get { return _showDownloadingUntil > DateTime.Now; } }

        public new void Add(string item)
        {
            base.Add(item);
            _showDownloadingUntil = DateTime.Now.AddSeconds(5);
        }
    }
}