using System;
using Nancy;

namespace Deployd.Agent.WebUi
{
    public static class NancyModuleExtensions
    {
        public static Response ViewOrJson<TViewModel>(this NancyModule module, string viewFile, TViewModel viewModel)
        {
            var defaultReponse = module.View[viewFile, viewModel];
            return module.Response.AsNegotiated(viewModel, defaultResponse: new Tuple<Func<Response>, string>(() => defaultReponse, "text/html"));
        }

        public static Response ResponseOrJson<TViewModel>(this NancyModule module, Response webResponse, TViewModel viewModel)
        {
            var defaultReponse = webResponse;
            return module.Response.AsNegotiated(viewModel, defaultResponse: new Tuple<Func<Response>, string>(() => defaultReponse, "text/html"));
        }
        public static Response ResponseOrJson(this NancyModule module, Response webResponse)
        {
            var defaultReponse = webResponse;
            return module.Response.AsNegotiated((object)null, defaultResponse: new Tuple<Func<Response>, string>(() => defaultReponse, "text/html"));
        }
    }
}