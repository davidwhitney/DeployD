using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace DeployD.Hub.Areas.Api.Code
{
    public class XmlRepresentationBuilder : IRepresentationBuilder
    {
        private static XmlWriterSettings _xmlWriterSettings;

        public XmlRepresentationBuilder()
        {
            _xmlWriterSettings = new XmlWriterSettings();
            _xmlWriterSettings.Encoding = Encoding.UTF8;
            _xmlWriterSettings.Indent = true;
            _xmlWriterSettings.NewLineHandling = NewLineHandling.Entitize;
            _xmlWriterSettings.NewLineOnAttributes = true;
        }

        public string BuildRepresentationOf<T>(T resource)
        {
            var isDataContract = typeof(T).GetCustomAttributes(typeof(DataContractAttribute), true).Length > 0;

            return isDataContract ? XmlByDataContract(resource) : XmlBySerializer(resource);
        }

        public string ContentType
        {
            get { return "application/xml"; }
        }

        private static string XmlBySerializer<T>(T resource)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            StringBuilder sb = new StringBuilder();
            using (XmlWriter writer = XmlWriter.Create(sb, _xmlWriterSettings))
            {
                serializer.Serialize(writer, resource);
                writer.Flush();
            }

            return sb.ToString();
        }

        private static string XmlByDataContract<T>(T resource)
        {
            var dcs = new DataContractSerializer(typeof(T));
            var sb = new StringBuilder();

            using (XmlWriter writer = XmlWriter.Create(sb, _xmlWriterSettings))
            {
                dcs.WriteStartObject(writer, resource);
                dcs.WriteObject(writer, resource);
                dcs.WriteEndObject(writer);
                writer.Flush();
            }
            return sb.ToString();
        }
    }
}