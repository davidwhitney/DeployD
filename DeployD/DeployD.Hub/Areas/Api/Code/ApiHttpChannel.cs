using System;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace DeployD.Hub.Areas.Api.Code
{
    public class ApiHttpChannel : IApiHttpChannel
    {
        private readonly IRepresentationBuilder[] _representationBuilders;
        public ApiHttpChannel(IRepresentationBuilder[] representationBuilders)
        {
            _representationBuilders = representationBuilders;
        }
        public ActionResult RepresentationOf<T>(T resource, HttpContextBase httpContext)
        {
            string[] acceptTypes = httpContext.Request.Headers["Accept"].Split(new[] {';'},
                                                                               StringSplitOptions.RemoveEmptyEntries);

            string contentType = AppropriateContentType(acceptTypes);

            return BuildRepresentationOf(resource, contentType);
            
        }

        private ActionResult BuildRepresentationOf<T>(T resource, string contentType)
        {
            var builder = _representationBuilders.FirstOrDefault(b => b.ContentType == contentType)
                ?? _representationBuilders.FirstOrDefault(b=>b.ContentType=="application/xml");

            string content = builder.BuildRepresentationOf(resource);
            ContentResult result = new ContentResult();
            result.ContentType = contentType;
            result.ContentEncoding = Encoding.UTF8;
            result.Content = content;
            return result;
        }

        private string AppropriateContentType(string[] acceptTypes)
        {
            return acceptTypes.FirstOrDefault() ?? "application/xml";
        }
    }
}