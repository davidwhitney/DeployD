using System;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Web;
using Deployd.Agent.WebUi.Modules;
using Deployd.Core.Hosting;
using Nancy.Hosting.Wcf;
using Ninject.Extensions.Logging;

namespace Deployd.Agent.Services.Management
{
    public class ManagementInterfaceHost : IWindowsService
    {
        private readonly ILogger _logger;

        public ManagementInterfaceHost(ILogger logger)
        {
            _logger = logger;
        }

        ~ManagementInterfaceHost()
        {
            _logger.Warn("Destroying a {0}", this.GetType());

        }
        
        private WebServiceHost _host;

        protected Uri WebUiAddress { get; set; }
        public ApplicationContext AppContext { get; set; }

        public void Start(string[] args)
        {
            HomeModule.Container = AppContext.Container;
            PackagesModule.Container = AppContext.Container;
            InstallationsModule.Container = AppContext.Container;
            LogModule.Container = AppContext.Container;
            ActionsModule.Container = AppContext.Container;

            Nancy.Json.JsonSettings.MaxJsonLength = 1024*1024*5; // 5mb max

            try
            {
                WebUiAddress = new Uri("http://localhost:9999/");
                _host = new WebServiceHost(new NancyWcfGenericService(), WebUiAddress);
                _host.AddServiceEndpoint(typeof (NancyWcfGenericService), new WebHttpBinding(), "");
                _host.Open();
            } 
            catch (Exception ex)
            {
                _logger.Fatal(ex, "could not start listening");
            }

            _logger.Info("Hosting Web interface on: " + WebUiAddress);
        }

        public void Stop()
        {
            _host.Close();
        }
    }
}
