using Nancy;
using TinyIoC;

namespace Deployd.Agent.WebUi
{
    public class NancyConventionsBootstrapper : DefaultNancyBootstrapper
    {
        protected override void ApplicationStartup(TinyIoCContainer container, Nancy.Bootstrapper.IPipelines pipelines)
        {
            Conventions.ViewLocationConventions.Add((viewName, model, context) => string.Concat("WebUi/Views/", viewName));
        }
    }
}
