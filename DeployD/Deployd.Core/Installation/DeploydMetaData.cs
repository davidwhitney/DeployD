using System.Xml.Linq;

namespace Deployd.Core.Installation
{
    public class DeployDMetaData
    {
        public DeployDMetaData(string nuspecFilePath)
        {
            XDocument xml = XDocument.Load(nuspecFilePath);
            var settings = xml.Element("settings");
            ServiceName = settings.Element("serviceName") != null ? settings.Element("serviceName").Value : "";
            IISPath = settings.Element("iisPath") != null ? settings.Element("iisPath").Value : "";
        }

        public string IISPath { get; set; }

        public string ServiceName { get; set; }
    }
}