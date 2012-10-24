using System;
using System.Collections.Generic;
using System.Linq;
using Deployd.Core.Hosting;

namespace Deployd.Core.Notifications
{
    public class NotificationService : IWindowsService, INotificationService
    {
        private readonly IEnumerable<INotifier> _notifiers;

        public NotificationService(IEnumerable<INotifier> notifiers)
        {
            _notifiers = notifiers.ToArray();
        }

        public void Start(string[] args)
        {
            foreach(var notifier in _notifiers)
            {
                notifier.OpenConnections();
            }
        }

        public void Stop()
        {
            foreach (var notifier in _notifiers)
            {
                if (notifier is IDisposable)
                {
                    ((IDisposable)notifier).Dispose();
                }
            }
        }

        public ApplicationContext AppContext { get; set; }
        public void NotifyAll(EventType eventType, string message)
        {
            message = System.Net.Dns.GetHostName() + " " + message;
            foreach(var notifier in _notifiers)
            {
                if (notifier.Handles(eventType))
                {
                    notifier.Notify(message);
                }
            }
        }
    }
}
