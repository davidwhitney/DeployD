using DeployD.Hub.Areas.Api.Code;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Embedded;
using log4net;

[assembly: WebActivator.PreApplicationStartMethod(typeof(DeployD.Hub.App_Start.NinjectWebCommon), "Start")]
[assembly: WebActivator.ApplicationShutdownMethodAttribute(typeof(DeployD.Hub.App_Start.NinjectWebCommon), "Stop")]

namespace DeployD.Hub.App_Start
{
    using System;
    using System.Web;

    using Microsoft.Web.Infrastructure.DynamicModuleHelper;

    using Ninject;
    using Ninject.Web.Common;

    public static class NinjectWebCommon 
    {
        private static readonly Bootstrapper bootstrapper = new Bootstrapper();

        /// <summary>
        /// Starts the application
        /// </summary>
        public static void Start() 
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
            bootstrapper.Initialize(CreateKernel);
        }
        
        /// <summary>
        /// Stops the application.
        /// </summary>
        public static void Stop()
        {
            bootstrapper.ShutDown();
        }
        
        /// <summary>
        /// Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
            kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();
            
            RegisterServices(kernel);
            return kernel;
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
            kernel.Bind<IApiHttpChannel>().To<ApiHttpChannel>();
            kernel.Bind<IRepresentationBuilder>().To<XmlRepresentationBuilder>();
            kernel.Bind<IRepresentationBuilder>().To<JsonRepresentationBuilder>();
            kernel.Bind<IAgentManager>().To<AgentManager>().InRequestScope();
            kernel.Bind<IPackageStore>().To<LocalPackageStore>();
            kernel.Bind<IAgentRemoteService>().To<AgentRemoteService>();
            kernel.Bind<ILog>().ToMethod(context =>
            {
                if (context.Request.Target != null)
                    return LogManager.GetLogger(context.Request.Target.Name);
                return LogManager.GetLogger("log");
            });

            kernel.Bind<IDocumentStore>()
                .ToMethod(ctx =>
                {
                    //var documentStore = new DocumentStore() { Url = "http://localhost:8080" };
                    var documentStore = new EmbeddableDocumentStore(){DataDirectory = "App_Data/Database"};
                    documentStore.Initialize();
                    return documentStore;
                }).InSingletonScope();


            kernel.Bind<IDocumentSession, DocumentSession>()
                .ToMethod(ctx =>
                {
                    ctx.Kernel.Get<ILog>().Debug("raven session opened");
                    var session = ctx.Kernel.Get<IDocumentStore>().OpenSession();
                    session.Advanced.UseOptimisticConcurrency = true;
                    return session as DocumentSession;

                })
                .InRequestScope()
                .OnDeactivation((ctx, session) =>
                {
                    if (session.Advanced.HasChanges)
                    {
                        session.SaveChanges();
                        ctx.Kernel.Get<ILog>().Debug("raven session changes saved");
                    }
                    ctx.Kernel.Get<ILog>().Debug("raven session closed");
                });
        }        
    }
}
