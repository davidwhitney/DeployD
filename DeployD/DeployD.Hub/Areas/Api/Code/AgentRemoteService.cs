using System.Collections.Generic;
using System.IO;
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

            var packages = Get<PackageListViewModel>(url);
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
    }

    public class PackageListViewModel
    {
        public List<PackageViewModel> Packages { get; set; } 
    }
}