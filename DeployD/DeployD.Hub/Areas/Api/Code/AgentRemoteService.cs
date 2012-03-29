using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using DeployD.Hub.Areas.Api.Models;
using DeployD.Hub.Areas.Api.Models.Dto;

namespace DeployD.Hub.Areas.Api.Code
{
    public class AgentRemoteService : IAgentRemoteService
    {
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
            using (var stream = response.GetResponseStream())
            using (var streamReader = new StreamReader(stream, System.Text.Encoding.UTF8))
            {
                responseContent = streamReader.ReadToEnd();
                contentType = response.Headers["Content-Type"];
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

    public class PackageLogFolderListDto
    {
        public List<LogListDto> PackageLogFolders { get; set; }
    }

    public class LogFileDto
    {
        public string LogFileName { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public string PackageId { get; set; }
        public string LogContents { get; set; }
        public string DateCreatedString { get { return DateCreated.ToString("dd-MM-yyyy HH:mm:ss"); } }
        public string DateModifiedString { get { return DateModified.ToString("dd-MM-yyyy HH:mm:ss"); } }
    }

    public class LogListDto
    {
        public List<LogDto> Logs { get; set; }
        public string PackageId { get; set; }
    }

    public class LogDto
    {
        public string LogFileName { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public string DateCreatedString { get { return DateCreated.ToString("dd-MM-yyyy HH:mm:ss"); } }
        public string DateModifiedString { get { return DateModified.ToString("dd-MM-yyyy HH:mm:ss"); } }
    }

    public class PackageListDto
    {
        public List<PackageViewModel> Packages { get; set; } 
    }
}