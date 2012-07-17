using System;
using System.Timers;
using Deployd.Core.Hosting;
using Ninject.Extensions.Logging;

namespace Deployd.Core.Remoting
{
    public class HubCommunicationsQueueService : IWindowsService
    {
        private ILogger _logger;
        private readonly HubCommunicationsQueue _communicationsQueue;
        Timer _timer = new Timer(1000);

        public HubCommunicationsQueueService(HubCommunicationsQueue communicationsQueue, ILogger logger)
        {
            _communicationsQueue = communicationsQueue;
            _logger = logger;
        }

        ~HubCommunicationsQueueService()
        {
            _logger.Warn("Destroying a {0}", this.GetType());
        }

        public void Start(string[] args)
        {
            _timer.Elapsed += ProcessCommunicationsQueue;
            _timer.Start();
            
        }

        private void ProcessCommunicationsQueue(object sender, ElapsedEventArgs e)
        {
            while(_communicationsQueue.Count > 0)
            {
                var task = _communicationsQueue.Dequeue();
                task.Start();
                task.Wait(TimeSpan.FromSeconds(20));
            }
        }

        public void Stop()
        {
            _timer.Stop();
        }

        public ApplicationContext AppContext { get; set; }
    }
}