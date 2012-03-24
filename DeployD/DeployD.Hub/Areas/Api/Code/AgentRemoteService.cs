using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using DeployD.Hub.Areas.Api.Models;

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

        public AgentViewModel GetAgentStatus(string hostname)
        {
            return Get<AgentViewModel>(string.Format("http://{0}:9999/sitrep", hostname));
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
    }

    public class PackageListViewModel
    {
        public List<PackageViewModel> Packages { get; set; } 
    }
}