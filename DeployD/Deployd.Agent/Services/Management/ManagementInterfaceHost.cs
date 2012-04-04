using System;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Web;
using Deployd.Agent.WebUi.Modules;
using Deployd.Core.Hosting;
using Nancy.Hosting.Wcf;
using log4net;

namespace Deployd.Agent.Services.Management
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
            PackagesModule.Container = AppContext.Container;
            InstallationsModule.Container = AppContext.Container;
            LogModule.Container = AppContext.Container;

            try
            {
                WebUiAddress = new Uri("http://localhost:9999/");
                _host = new WebServiceHost(new NancyWcfGenericService(), WebUiAddress);
                _host.AddServiceEndpoint(typeof (NancyWcfGenericService), new WebHttpBinding(), "");
                _host.Open();
            } 
            catch (Exception ex)
            {
                Logger.Fatal("could not start listening", ex);
            }

            Logger.Info("Hosting Web interface on: " + WebUiAddress);
        }

        public void Stop()
        {
            _host.Close();
        }
    }
}
