using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.ServiceProcess;
using System.Threading.Tasks;
using Deployd.Core.AgentConfiguration;
using Ninject.Extensions.Logging;
using log4net;

namespace Deployd.Core.Hosting
{
    public class WindowsServiceRunner
    {
        private readonly Action<ApplicationContext, string> _notify;
        private readonly string[] _args;
		private readonly ApplicationContext _context;
        private readonly Func<IWindowsService[]> _createServices;
        private readonly IAgentSettingsManager _agentSettingsManager=null;
        private readonly ILog _logger;
        private readonly Action<ApplicationContext> _configureContext;

        /// <summary>
        /// Executes the provided IWindowsServices and supports automatic installation using the command line params -install / -uninstall
        /// </summary>
        /// <param name="args"></param>
        /// <param name="createServices">Function which provides a WindowsServiceCollection of services to execute</param>
        /// <param name="configureContext">Optional application context configuration</param>
        /// <param name="installationSettings">Optional installer configuration with semi-sensible defaults</param>
        /// <param name="registerContainer">Optionally register an IoC container</param>
        /// <param name="agentSettingsManager">Optionally provide agent settings </param>
        public WindowsServiceRunner(string[] args,
                                    Func<IWindowsService[]> createServices, 
                                    Action<ApplicationContext> configureContext = null,
                                    Action<System.ServiceProcess.ServiceInstaller, 
                                    ServiceProcessInstaller> installationSettings = null,
                                    Func<IIocContainer> registerContainer = null,
                                    IAgentSettingsManager agentSettingsManager = null,
                                    Action<ApplicationContext,string> notify=null)
        {
            _notify = notify ?? ((ctx,message) => { });
            var log = LogManager.GetLogger(typeof (WindowsServiceRunner));
            _args = args;
			_context = new ApplicationContext();
            _createServices = createServices;
            _agentSettingsManager = agentSettingsManager;
            _logger = log;
            _configureContext = configureContext ?? (ctx => {  });
    	    _context.ConfigureInstall = installationSettings ?? ((serviceInstaller, serviceProcessInstaller) => { });
            _context.Container = registerContainer;

            if (registerContainer==null)
            {
                throw new ArgumentException("Binding container is null");
            }
            if (registerContainer != null)
            {
                _logger.DebugFormat("container is " + registerContainer.ToString());
            }
        }

		public void Host()
        {
            var services = CreateHostableServices();

        	_configureContext(_context);
		    _context.Services = services;

            ServiceInstaller.PerformAnyRequestedInstallations(_args, _context);
            Execute(services, _args);
        }

        private IWindowsService[] CreateHostableServices()
        {
            return _createServices();
        }

        private void Execute(IWindowsService[] services, string[] args)
        {
            NotifyApplicationStartup();

			try
			{
				LaunchInteractiveServices(services, args);
				LaunchNonInteractiveServices(services);
			}
			catch(Exception ex)
			{
				_context.Log(ex.ToString());
				throw;
			}
		}

        private void NotifyApplicationStartup()
        {
            _context.Log("================================================");
            _context.Log("================================================");
            _context.Log("Application Started.");
            _context.Log("================================================");
            _context.Log("================================================");

            var currentApplication = Assembly.GetEntryAssembly();
            var name = currentApplication.GetName();
            var fileInfo = new FileInfo(currentApplication.Location);

            _context.Log("================================================");
            _context.Log("Entry Assembly: " + name.Name);
            _context.Log("Version: " + name.Version);
            _context.Log("Created: " + fileInfo.CreationTime);
            _context.Log("Last Accessed: " + fileInfo.LastAccessTime);
            _context.Log("Last Written: " + fileInfo.LastWriteTime);
            _context.Log("Attributes: " + fileInfo.Attributes);
            _context.Log("Codebase: " + currentApplication.EscapedCodeBase);
            _context.Log("CLR Version: " + currentApplication.ImageRuntimeVersion);
            _context.Log("Location: " + currentApplication.Location);
            if (_agentSettingsManager != null)
            {
                _context.Log("Environment: " + _agentSettingsManager.Settings.DeploymentEnvironment);
                _context.Log("Hub Address: " + _agentSettingsManager.Settings.HubAddress);
            }
            _context.Log("================================================");
        }

        private void LaunchInteractiveServices(IEnumerable<IWindowsService> services, string[] args)
		{
			if (!Environment.UserInteractive)
			{
				return;
			}

			foreach(var service in services)
			{
				_context.Log("Starting service: " + service);
			    service.AppContext = _context;
				var service1 = service;
				var task = new Task(() => service1.Start(args));
				task.Start();
			}

			_context.Log("Listening..");
            string command = "";
            do
            {
                command = Console.ReadLine();
                if (command == "quit")
                    break;
                else if (command=="notify")
                {
                    _notify(_context, "Test notification from deployment agent");
                    _logger.Info("Test notification sent");
                }
            } while(true);

            foreach (var service in services)
            {
                _context.Log("Stopping service: " + service);
                service.AppContext = _context;
                var service1 = service;
                service1.Stop();
            }
		}

        private void LaunchNonInteractiveServices(IWindowsService[] services)
		{
			if (Environment.UserInteractive)
			{
				return;
			}

			var wrapped = Wrap(services);
			ServiceBase.Run(wrapped);

			_context.Log("Listening as a service...");
		}

        private ServiceContainer Wrap(IWindowsService[] services)
    	{
    		foreach (var service in services)
			{
				_context.Log("Building wrapper for : " + service);
    			service.AppContext = _context;
    		}

            return new ServiceContainer(services) {AppContext = _context};
    	}
    }
}