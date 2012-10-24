﻿using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using Deployd.Agent.Conventions;
using Deployd.Core;
using Deployd.Core.AgentConfiguration;
using Deployd.Core.Hosting;
using Deployd.Core.Notifications;
using Ninject;
using Ninject.Modules;
using log4net;
using log4net.Appender;
using log4net.Config;
using ServiceInstaller = Deployd.Core.Hosting.ServiceInstaller;

namespace Deployd.Agent
{
    public class Program
    {
        private const string NAME = "Deployd.Agent";

        protected static ILog Logger;
        private static IKernel _kernel;
        private static ContainerWrapper _containerWrapper;

        static void Main(string[] args)
        {
            XmlConfigurator.Configure();

            try
            {
                Logger = LogManager.GetLogger(typeof (Program));
                _kernel = new StandardKernel(new INinjectModule[] {new ContainerConfiguration()});
                
                _containerWrapper = new ContainerWrapper(_kernel);

                var agentSettingsManager = _containerWrapper.GetType<IAgentSettingsManager>();

                SetLogAppenderPaths(agentSettingsManager.Settings, LogManager.GetLogger("Agent.Main"));

                var notificationService = _containerWrapper.GetType<INotificationService>();

                new WindowsServiceRunner(args,
                                        () => _kernel.GetAll<IWindowsService>().ToArray(),
                                            installationSettings: (serviceInstaller, serviceProcessInstaller) =>
                                                                {
                                                                    serviceInstaller.ServiceName = NAME;
                                                                    serviceInstaller.StartType =
                                                                        ServiceStartMode.Manual;
                                                                    serviceProcessInstaller.Account =
                                                                        ServiceAccount.User;
                                                                },
                                        registerContainer: () => _containerWrapper,
                                        configureContext: x => { x.Log = s => Logger.Info(s); },
                                        agentSettingsManager:agentSettingsManager,
                                        notify: (x,message)=> notificationService.NotifyAll(EventType.SystemEvents, message))
                .Host();
 } catch (Exception ex)
            {
                Logger.Error("Unhandled exception", ex);
            }
        }

        private static void SetLogAppenderPaths(IAgentSettings agentSettings, ILog log)
        {
            var appenders = log.Logger.Repository.GetAppenders().Where(a => a is FileAppender);
            foreach (FileAppender appender in appenders)
            {
                string fileName = Path.GetFileName(appender.File);
                appender.File = Path.Combine(agentSettings.LogsDirectory.MapVirtualPath(), fileName);
                appender.ActivateOptions();
            }
        }
    }

    [RunInstaller(true)]
    public class Installer : ServiceInstaller
    {
    }

    public class ContainerWrapper : IIocContainer
    {
        private readonly IKernel _kernel;

        public ContainerWrapper(IKernel kernel)
        {
            _kernel = kernel;
        }

        public T GetType<T>()
        {
            return _kernel.Get<T>();
        }
    }
}
