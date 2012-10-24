using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using DeployD.Hub.Areas.Api.Models;
using Deployd.Core;
using Ninject.Extensions.Logging;

namespace DeployD.Hub.Areas.Api.Code
{
    public class AgentRemoteService : IAgentRemoteService
    {
        private readonly ILogger _logger;

        public AgentRemoteService(ILogger logger)
        {
            _logger = logger;
        }

        public List<PackageViewModel> ListPackages(string hostname)
        {
            string url = string.Format("http://{0}:9999/packages", hostname);

            var packages = Get<PackageListDto>(url);
            return packages.Packages;
        }

        public AgentStatusReport GetAgentStatus(string hostname)
        {
            return Get<AgentStatusReport>(string.Format("http://{0}:9999/sitrep", hostname));
        }

        private static T Get<T>(string url)
        {
            string responseContent;
            HttpWebRequest request = HttpWebRequest.Create(url) as HttpWebRequest;
            request.Accept = "application/json";
            string contentType = "";
            using (var response = request.GetResponse() as HttpWebResponse)
            using (var stream = response.GetResponseStream())
            using (var streamReader = new StreamReader(stream, System.Text.Encoding.UTF8))
            {
                responseContent = streamReader.ReadToEnd();
                contentType = response.Headers["Content-Type"];
            }
            var decoded = System.Web.Helpers.Json.Decode<T>(responseContent);
            return decoded;
        }

        public void StartUpdatingAllPackages(string hostname, string version)
        {
            string responseContent;
            string url = string.Format("http://{0}:9999/packages/UpdateAllTo/"+version, hostname);
            HttpWebRequest request = HttpWebRequest.Create(url) as HttpWebRequest;

            var data = "specificVersion=" + version;
            var dataBytes = Encoding.UTF8.GetBytes(data);
            request.Method = "POST";
            request.ContentLength = 0;

            request.Accept = "application/json";
            string contentType = "";
            using (var response = request.GetResponse() as HttpWebResponse)
            {
                if (response.StatusCode == HttpStatusCode.InternalServerError)
                {
                    _logger.Debug("UpdateAll - agent had internal error");
                }
                else
                {
                    _logger.Debug("UpdateAll - agent returned {0}: {1}", response.StatusCode, response.StatusDescription);
                    using (var stream = response.GetResponseStream())
                    using (var streamReader = new StreamReader(stream, System.Text.Encoding.UTF8))
                    {
                        responseContent = streamReader.ReadToEnd();
                        contentType = response.Headers["Content-Type"];
                        _logger.Debug(responseContent);
                    }
                }
            }


        }

        public void StartUpdate(string hostname, string packageId, string version)
        {
            string url = string.Format("http://{0}:9999/packages/{1}/install/{2}", hostname, packageId, version);
            HttpWebRequest request = HttpWebRequest.Create(url) as HttpWebRequest;
            request.Method = "POST";
            request.ContentLength = 0;

            request.Accept = "application/json";
            using (var response = request.GetResponse() as HttpWebResponse)
            {
            }
        }

        public List<LogListDto> ListPackagesWithLogs(string hostname)
        {
            string url = string.Format("http://{0}:9999/log", hostname);
            var packages = Get<string[]>(url);
            List<LogListDto> dto = packages.Select(p => new LogListDto() {PackageId = p}).ToList();
            return dto;
        }

        public List<LogDto> ListLogsForPackage(string hostname, string packageId)
        {
            string url = string.Format("http://{0}:9999/log/{1}", hostname, packageId);
            var logList = Get<LogListDto>(url);
            return logList.Logs;
        }

        public LogFileDto GetLogFile(string hostname, string packageId, string filename)
        {
            string url = string.Format("http://{0}:9999/log/{1}/{2}", hostname, packageId, filename);
            return Get<LogFileDto>(url);
        }
    }
    [DataContract]
    public class PackageLogFolderListDto
    {
        [DataMember]
        public List<LogListDto> PackageLogFolders { get; set; }
    }

    [DataContract]
    public class LogFileDto
    {
        [DataMember]
        public string LogFileName { get; set; }
        [DataMember]
        public DateTime DateCreated { get; set; }
        [DataMember]
        public DateTime DateModified { get; set; }
        [DataMember]
        public string PackageId { get; set; }
        [DataMember]
        public string LogContents { get; set; }
        [DataMember]
        public string DateCreatedString { get { return DateCreated.ToString("dd-MM-yyyy HH:mm:ss"); } set{} }
        [DataMember]
        public string DateModifiedString { get { return DateModified.ToString("dd-MM-yyyy HH:mm:ss"); } set { } }
    }

    [DataContract]
    public class LogListDto
    {
        [DataMember]
        public List<LogDto> Logs { get; set; }
        [DataMember]
        public string PackageId { get; set; }
    }

    [DataContract]
    public class LogDto
    {
        [DataMember]
        public string LogFileName { get; set; }
        [DataMember]
        public DateTime DateCreated { get; set; }
        [DataMember]
        public DateTime DateModified { get; set; }
        [DataMember]
        public string DateCreatedString { get { return DateCreated.ToString("dd-MM-yyyy HH:mm:ss"); } set { } }
        [DataMember]
        public string DateModifiedString { get { return DateModified.ToString("dd-MM-yyyy HH:mm:ss"); } set { } }
    }

    [DataContract]
    public class PackageListDto
    {
        [DataMember]
        public List<PackageViewModel> Packages { get; set; } 
    }
}