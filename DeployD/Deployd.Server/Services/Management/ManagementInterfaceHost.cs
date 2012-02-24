using System;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Web;
using Deployd.Core.Hosting;
using Deployd.Server.WebUi.Modules;
using Nancy.Hosting.Wcf;
using log4net;

namespace Deployd.Server.Services.Management
{
    public class ManagementInterfaceHost : IWindowsService
    {
        protected static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType); 
        
        private WebServiceHost _host;

        protected Uri WebUiAddress { get; set; }
        public ApplicationContext AppContext { get; set; }

        public void Start(string[] args)
        {
            HomeModule.Container = AppContext.Container;
            WebUiAddress = new Uri("http://localhost:8999/");
            _host = new WebServiceHost(new NancyWcfGenericService(), WebUiAddress);
            _host.AddServiceEndpoint(typeof(NancyWcfGenericService), new WebHttpBinding(), "");
            _host.Open();

            Logger.Info("Hosting Web interface on: " + WebUiAddress);
        }

        public void Stop()
        {
            _host.Close();
        }

    }
}
