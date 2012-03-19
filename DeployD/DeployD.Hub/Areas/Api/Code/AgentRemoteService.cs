using System.Collections.Generic;
using System.IO;
using System.Net;
using DeployD.Hub.Areas.Api.Models;

namespace DeployD.Hub.Areas.Api.Code
{
    public class AgentRemoteService : IAgentRemoteService
    {
        public IEnumerable<PackageViewModel> ListPackages(string hostname)
        {
            HttpWebRequest request = HttpWebRequest.Create(string.Format("http://{0}:9999/packages", hostname)) as HttpWebRequest;
            request.Accept = "application/json";
            string responseContent = "";
            string contentType = "";
            using (var response = request.GetResponse() as HttpWebResponse)
            using (var stream = response.GetResponseStream())
            using (var streamReader = new StreamReader(stream, System.Text.Encoding.UTF8))
            {
                responseContent = streamReader.ReadToEnd();
                contentType = response.Headers["Content-Type"];
            }

            var packages = System.Web.Helpers.Json.Decode<PackageListViewModel>(responseContent);
            return packages.Packages;
        }
    }

    public class PackageListViewModel
    {
        public List<PackageViewModel> Packages { get; set; } 
    }
}