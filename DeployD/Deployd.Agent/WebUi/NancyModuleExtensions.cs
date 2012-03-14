using System;
using Nancy;

namespace Deployd.Agent.WebUi.Modules
{
    public static class NancyModuleExtensions
    {
        public static Response ViewOrJson<TViewModel>(this NancyModule module, string viewFile, TViewModel viewModel)
        {
            var defaultReponse = module.View[viewFile, viewModel];
            return module.Response.AsNegotiated(viewModel, defaultResponse: new Tuple<Func<Response>, string>(() => defaultReponse, "text/html"));
        }
    }
}