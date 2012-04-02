using System.Web.Helpers;

namespace DeployD.Hub.Areas.Api.Code
{
    public class JsonRepresentationBuilder : IRepresentationBuilder
    {
        public string BuildRepresentationOf<T>(T resource)
        {
            return Json.Encode(resource);
        }

        public string ContentType
        {
            get { return "application/json"; }
        }
    }
}