using System.IO;
using System.Text;
using System.Web.Helpers;

namespace DeployD.Hub.Areas.Api.Code
{
    public class JsonRepresentationBuilder : IRepresentationBuilder
    {
        public string BuildRepresentationOf<T>(T resource)
        {
            var serialiser = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof (T));
            using (var outstream = new MemoryStream())
            {
                serialiser.WriteObject(outstream, resource);
                outstream.Flush();

                outstream.Position = 0;
                byte[] data = new byte[outstream.Length];
                outstream.Read(data, 0, data.Length);
                return Encoding.UTF8.GetString(data);
            }
        }

        public string ContentType
        {
            get { return "application/json"; }
        }
    }
}